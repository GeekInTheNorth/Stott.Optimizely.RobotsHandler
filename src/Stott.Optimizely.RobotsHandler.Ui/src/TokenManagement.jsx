import { useState, useEffect, useCallback } from 'react';
import PropTypes from 'prop-types';
import axios from 'axios';
import { Container, Button, Modal, Form } from 'react-bootstrap';
import { apiEndpoints } from './apiEndpointsData';

function TokenManagement(props) {
    const { showToastNotificationEvent } = props;
    const [tokens, setTokens] = useState([]);
    const [showAddModal, setShowAddModal] = useState(false);
    const [showDeleteModal, setShowDeleteModal] = useState(false);
    const [showApiInfoModal, setShowApiInfoModal] = useState(false);
    const [selectedApiInfo, setSelectedApiInfo] = useState(null);
    const [selectedToken, setSelectedToken] = useState(null);
    const [isMounted, setIsMounted] = useState(false);
    const [formData, setFormData] = useState({
        id: '',
        name: '',
        robotsScope: 'None',
        llmsScope: 'None',
        token: ''
    });

    const scopeOptions = [
        { value: 'None', label: 'None' },
        { value: 'Read', label: 'Read' },
        { value: 'Write', label: 'Write' }
    ];

    const handleShowFailureToast = useCallback((title, description) => 
        showToastNotificationEvent && showToastNotificationEvent(false, title, description), [showToastNotificationEvent]);

    const handleShowSuccessToast = useCallback((title, description) => 
        showToastNotificationEvent && showToastNotificationEvent(true, title, description), [showToastNotificationEvent]);

    const handleShowApiInfo = (apiKey) => {
        setSelectedApiInfo(apiEndpoints[apiKey]);
        setShowApiInfoModal(true);
    };

    const loadTokens = useCallback(async () => {
        try {
            // This would be replaced with your actual API endpoint
            const response = await axios.get(import.meta.env.VITE_APP_OPALTOKEN_LIST);
            if (response.data && Array.isArray(response.data)) {
                setTokens(response.data);
            } else {
                handleShowFailureToast('Failure', 'Failed to retrieve tokens data.');
            }
        } catch (error) {
            handleShowFailureToast('Failure', 'Failed to retrieve tokens data.');
        }
    }, [handleShowFailureToast]);

    const generateRandomToken = () => {
        const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
        let result = '';
        for (let i = 0; i < 32; i++) {
            result += chars.charAt(Math.floor(Math.random() * chars.length));
        }
        return result;
    };

    const handleAddToken = () => {
        setFormData({
            id: '',
            name: '',
            robotsScope: 'None',
            llmsScope: 'None',
            token: generateRandomToken()
        });
        setShowAddModal(true);
    };

    const handleDeleteToken = (token) => {
        setSelectedToken(token);
        setShowDeleteModal(true);
    };

    const handleSaveToken = async () => {
        // Validate required fields
        if (!formData.name || formData.name.trim() === '') {
            handleShowFailureToast('Validation Error', 'Token Name is required.');
            return;
        }

        try {
            let params = new URLSearchParams();

            params.append('robotsScope', formData.robotsScope);
            params.append('llmsScope', formData.llmsScope);
            params.append('name', formData.name);
            params.append('token', formData.token);

            await axios.post(import.meta.env.VITE_APP_OPALTOKEN_SAVE, params);
            handleShowSuccessToast('Success', 'Token created successfully.');
            
            setShowAddModal(false);
            loadTokens();
        } catch (error) {
            handleShowFailureToast('Failure', 'Failed to save token.');
        }
    };

    const handleDeleteConfirm = async () => {
        try {
            let url = ''.concat(import.meta.env.VITE_APP_OPALTOKEN_DELETE, selectedToken.id, '/');
            await axios.delete(url);
            handleShowSuccessToast('Success', 'Token deleted successfully.');
            setShowDeleteModal(false);
            loadTokens();
        } catch (error) {
            handleShowFailureToast('Failure', 'Failed to delete token.');
        }
    };

    const handleRegenerateToken = () => {
        setFormData(prev => ({
            ...prev,
            token: generateRandomToken()
        }));
    };

    const renderTokensList = () => {
        return tokens.map((token, index) => (
            <tr key={index}>
                <td>{token.name || token.id}</td>
                <td>{token.robotsScope}</td>
                <td>{token.llmsScope}</td>
                <td>
                    <code className="text-muted">{token.token}</code>
                </td>
                <td>
                    <Button variant='outline-danger' size='sm'onClick={() => handleDeleteToken(token)}>Delete</Button>
                </td>
            </tr>
        ));
    };

    useEffect(() => {
        if (!isMounted)
        {
            loadTokens();
            setIsMounted(true);
        }
    }, [isMounted, loadTokens]);

    return (
        <Container className='mt-3'>
            <div className='mb-2'>
                <h2>Opal Token Management</h2>
                <p>Manage authorization bearer tokens for accessing the Robots Handler functionality. Each token can have a read or read/write scope that determines the endpoints they grant access to. These endpoints have been optimized for compatability with Opal.</p>
                <p>Please note that <strong>token values</strong> are only visible during initial creation and cannot be retrieved later.</p>
                <h3 className='h5'>Available API Endpoints:</h3>
                <ul className="list-unstyled">
                    <li className="d-flex align-items-center mb-1">
                        <span><strong>Discovery API:</strong> /stott.robotshandler/opal/discovery/</span>
                        <Button variant="link" size="lg" className="p-0 ms-2 text-decoration-none" onClick={() => handleShowApiInfo('discovery')} title="View API Details">&#9432;</Button>
                    </li>
                    <li className="d-flex align-items-center mb-1">
                        <span><strong>Get robots.txt API:</strong> /stott.robotshandler/opal/tools/get-robot-txt-configurations/</span>
                        <Button variant="link" size="lg" className="p-0 ms-2 text-decoration-none" onClick={() => handleShowApiInfo('getRobots')} title="View API Details">&#9432;</Button>
                    </li>
                    <li className="d-flex align-items-center mb-1">
                        <span><strong>Save robots.txt API:</strong> /stott.robotshandler/opal/tools/save-robot-txt-configuration/</span>
                        <Button variant="link" size="lg" className="p-0 ms-2 text-decoration-none" onClick={() => handleShowApiInfo('saveRobots')} title="View API Details">&#9432;</Button>
                    </li>
                    <li className="d-flex align-items-center mb-1">
                        <span><strong>Get llms.txt API:</strong> /stott.robotshandler/opal/tools/get-llms-txt-configurations/</span>
                        <Button variant="link" size="lg" className="p-0 ms-2 text-decoration-none" onClick={() => handleShowApiInfo('getLlms')} title="View API Details">&#9432;</Button>
                    </li>
                    <li className="d-flex align-items-center mb-1">
                        <span><strong>Save llms.txt API:</strong> /stott.robotshandler/opal/tools/save-llms-txt-configuration/</span>
                        <Button variant="link" size="lg" className="p-0 ms-2 text-decoration-none" onClick={() => handleShowApiInfo('saveLlms')} title="View API Details">&#9432;</Button>
                    </li>
                </ul>
                <Button variant='primary' onClick={handleAddToken} className='text-nowrap'>Add Token</Button>
            </div>
            <div>
                <table className='table table-striped'>
                    <thead>
                        <tr>
                            <th className='table-header-fix'>Name</th>
                            <th className='table-header-fix'>Robots Scope</th>
                            <th className='table-header-fix'>LLMS Scope</th>
                            <th className='table-header-fix'>Token</th>
                            <th className='table-header-fix'>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {renderTokensList()}
                    </tbody>
                </table>
            </div>

            {/* Create Modal */}
            <Modal show={showAddModal} size='lg'>
                <Modal.Header closeButton onClick={() => { setShowAddModal(false); }}>
                    <Modal.Title>
                        Create New Token
                    </Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <Form>
                        {/* Hidden ID field - will be set by API */}
                        <input type="hidden" name="id" value={formData.id} />
                        
                        <Form.Group className='mb-3'>
                            <Form.Label>Token Name *</Form.Label>
                            <Form.Control type='text' value={formData.name} onChange={(e) => setFormData(prev => ({ ...prev, name: e.target.value }))} placeholder='Enter a descriptive name for this token' required />
                        </Form.Group>
                        <Form.Group className='mb-3'>
                            <Form.Label>Robots Scope</Form.Label>
                            <Form.Select value={formData.robotsScope} onChange={(e) => setFormData(prev => ({ ...prev, robotsScope: e.target.value }))}>
                                {scopeOptions.map((option, index) => ( <option key={index} value={option.value}>{option.label}</option> ))}
                            </Form.Select>
                        </Form.Group>
                        <Form.Group className='mb-3'>
                            <Form.Label>LLMS Scope</Form.Label>
                            <Form.Select value={formData.llmsScope} onChange={(e) => setFormData(prev => ({ ...prev, llmsScope: e.target.value }))}>
                                {scopeOptions.map((option, index) => ( <option key={index} value={option.value}>{option.label}</option> ))}
                            </Form.Select>
                        </Form.Group>
                        <Form.Group className='mb-3'>
                            <Form.Label>Token</Form.Label>
                            <div className='d-flex'>
                                <Form.Control type='text' value={formData.token} onChange={(e) => setFormData(prev => ({ ...prev, token: e.target.value }))} placeholder='Token value' readOnly />
                                <Button variant='outline-secondary' className='ms-2' onClick={handleRegenerateToken}>Regenerate</Button>
                            </div>
                            <Form.Text className='text-muted'>
                                Token is automatically generated. Click &quot;Regenerate&quot; to create a new random token.
                            </Form.Text>
                        </Form.Group>
                    </Form>
                </Modal.Body>
                <Modal.Footer>
                    <Button variant='primary' onClick={handleSaveToken}>
                        Create Token
                    </Button>
                    <Button variant='secondary' onClick={() => { setShowAddModal(false); }}>Cancel</Button>
                </Modal.Footer>
            </Modal>

            {/* Delete Confirmation Modal */}
            <Modal show={showDeleteModal} onHide={() => setShowDeleteModal(false)}>
                <Modal.Header closeButton>
                    <Modal.Title>Confirm Delete</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    Are you sure you want to delete the token &quot;{selectedToken?.name || selectedToken?.id}&quot;? This action cannot be undone.
                </Modal.Body>
                <Modal.Footer>
                    <Button variant='danger' onClick={handleDeleteConfirm}>Delete</Button>
                    <Button variant='secondary' onClick={() => setShowDeleteModal(false)}>Cancel</Button>
                </Modal.Footer>
            </Modal>

            {/* API Information Modal */}
            <Modal show={showApiInfoModal} onHide={() => setShowApiInfoModal(false)} size="lg">
                <Modal.Header closeButton>
                    <Modal.Title>API Information: {selectedApiInfo?.name}</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    {selectedApiInfo && (
                        <div>
                            <div className="mb-3">
                                <h6><strong>HTTP Method:</strong></h6>
                                <span className={`badge ${selectedApiInfo.httpMethod === 'GET' ? 'bg-success' : 'bg-primary'}`}>
                                    {selectedApiInfo.httpMethod}
                                </span>
                            </div>
                            
                            <div className="mb-3">
                                <h6><strong>URL:</strong></h6>
                                <code className="d-block p-2 bg-light border rounded">{selectedApiInfo.url}</code>
                            </div>

                            <div className="mb-3">
                                <h6><strong>Request JSON:</strong></h6>
                                <pre className="bg-light border rounded p-2 small overflow-auto" style={{maxHeight: '200px'}}>
                                    <code>{selectedApiInfo.requestJson}</code>
                                </pre>
                            </div>

                            <div className="mb-3">
                                <h6><strong>Response JSON:</strong></h6>
                                <pre className="bg-light border rounded p-2 small overflow-auto" style={{maxHeight: '300px'}}>
                                    <code>{selectedApiInfo.responseJson}</code>
                                </pre>
                            </div>

                            {selectedApiInfo.showAuthorization && (
                                <div className="alert alert-info">
                                    <strong>Note:</strong> Remember to include the <code>Authorization: Bearer YOUR_TOKEN</code> header when making requests to this endpoint.
                                </div>
                            )}
                        </div>
                    )}
                </Modal.Body>
                <Modal.Footer>
                    <Button variant='secondary' onClick={() => setShowApiInfoModal(false)}>Close</Button>
                </Modal.Footer>
            </Modal>
        </Container>
    );
}

export default TokenManagement;

TokenManagement.propTypes = {
    showToastNotificationEvent: PropTypes.func
};
