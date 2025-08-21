import { useState, useEffect } from 'react';
import axios from 'axios';
import { Alert, Container, Row, Button, Modal, Form, Badge } from 'react-bootstrap';

function TokenManagement(props) {
    const [tokens, setTokens] = useState([]);
    const [showAddModal, setShowAddModal] = useState(false);
    const [showEditModal, setShowEditModal] = useState(false);
    const [showDeleteModal, setShowDeleteModal] = useState(false);
    const [selectedToken, setSelectedToken] = useState(null);
    const [formData, setFormData] = useState({
        id: '',
        scope: 'Read',
        token: ''
    });

    const scopeOptions = [
        { value: 'Read', label: 'Read' },
        { value: 'Write', label: 'Write' }
    ];

    useEffect(() => {
        loadTokens();
    }, []);

    const handleShowFailureToast = (title, description) => 
        props.showToastNotificationEvent && props.showToastNotificationEvent(false, title, description);

    const handleShowSuccessToast = (title, description) => 
        props.showToastNotificationEvent && props.showToastNotificationEvent(true, title, description);

    const loadTokens = async () => {
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
    };

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
            scope: 'Read',
            token: generateRandomToken()
        });
        setShowAddModal(true);
    };

    const handleEditToken = (token) => {
        setSelectedToken(token);
        setFormData({
            id: token.id,
            scope: token.scope,
            token: token.token
        });
        setShowEditModal(true);
    };

    const handleDeleteToken = (token) => {
        setSelectedToken(token);
        setShowDeleteModal(true);
    };

    const handleSaveToken = async () => {
        try {
            if (showAddModal) {
                // Add new token
                await axios.post(import.meta.env.VITE_APP_OPALTOKEN_SAVE, formData);
                handleShowSuccessToast('Success', 'Token created successfully.');
            } else {
                // Update existing token
                await axios.put(`/api/tokens/${formData.id}`, formData);
                handleShowSuccessToast('Success', 'Token updated successfully.');
            }
            
            setShowAddModal(false);
            setShowEditModal(false);
            loadTokens();
        } catch (error) {
            handleShowFailureToast('Failure', 'Failed to save token.');
        }
    };

    const handleDeleteConfirm = async () => {
        try {
            let url = ''.concat(import.meta.env.VITE_APP_ROBOTS_DELETE, selectedToken.id, '/');
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
                <td>{token.id}</td>
                <td>
                    <Badge bg={token.scope === 'Read' ? 'info' : 'warning'}>
                        {token.scope}
                    </Badge>
                </td>
                <td>
                    <code className="text-muted">{token.token.substring(0, 8)}...</code>
                </td>
                <td>
                    <Button 
                        variant='outline-primary' 
                        size='sm' 
                        className='me-2'
                        onClick={() => handleEditToken(token)}
                    >
                        Edit
                    </Button>
                    <Button 
                        variant='outline-danger' 
                        size='sm'
                        onClick={() => handleDeleteToken(token)}
                    >
                        Delete
                    </Button>
                </td>
            </tr>
        ));
    };

    return (
        <Container className='mt-3'>
            <Row className='mb-2'>
                <div className='col-xl-9 col-lg-9 col-sm-12 col-xs-12 p-0'>
                    <Alert variant='primary' className='p-3'>
                        Manage API tokens for accessing the Robots Handler functionality. Each token has a specific scope that determines its permissions.
                    </Alert>
                </div>
                <div className='col-xl-3 col-lg-3 col-sm-12 col-xs-12 p-0 text-end'>
                    <Button variant='primary' onClick={handleAddToken} className='text-nowrap p-3'>
                        Add Token
                    </Button>
                </div>
            </Row>
            <Row>
                <table className='table table-striped'>
                    <thead>
                        <tr>
                            <th className='table-header-fix'>ID</th>
                            <th className='table-header-fix'>Scope</th>
                            <th className='table-header-fix'>Token</th>
                            <th className='table-header-fix'>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {renderTokensList()}
                    </tbody>
                </table>
            </Row>

            {/* Add/Edit Modal */}
            <Modal show={showAddModal || showEditModal} size='lg'>
                <Modal.Header closeButton onClick={() => { setShowAddModal(false); setShowEditModal(false); }}>
                    <Modal.Title>
                        {showAddModal ? 'Create New Token' : 'Edit Token'}
                    </Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <Form>
                        <Form.Group className='mb-3'>
                            <Form.Label>ID</Form.Label>
                            <Form.Control
                                type='text'
                                value={formData.id}
                                onChange={(e) => setFormData(prev => ({ ...prev, id: e.target.value }))}
                                placeholder='Enter token ID'
                            />
                        </Form.Group>
                        <Form.Group className='mb-3'>
                            <Form.Label>Scope</Form.Label>
                            <Form.Select
                                value={formData.scope}
                                onChange={(e) => setFormData(prev => ({ ...prev, scope: e.target.value }))}
                            >
                                {scopeOptions.map((option, index) => (
                                    <option key={index} value={option.value}>
                                        {option.label}
                                    </option>
                                ))}
                            </Form.Select>
                        </Form.Group>
                        <Form.Group className='mb-3'>
                            <Form.Label>Token</Form.Label>
                            <div className='d-flex'>
                                <Form.Control
                                    type='text'
                                    value={formData.token}
                                    onChange={(e) => setFormData(prev => ({ ...prev, token: e.target.value }))}
                                    placeholder='Token value'
                                    readOnly
                                />
                                <Button 
                                    variant='outline-secondary' 
                                    className='ms-2'
                                    onClick={handleRegenerateToken}
                                >
                                    Regenerate
                                </Button>
                            </div>
                            <Form.Text className='text-muted'>
                                Token is automatically generated. Click &quot;Regenerate&quot; to create a new random token.
                            </Form.Text>
                        </Form.Group>
                    </Form>
                </Modal.Body>
                <Modal.Footer>
                    <Button variant='primary' onClick={handleSaveToken}>
                        {showAddModal ? 'Create Token' : 'Save Changes'}
                    </Button>
                    <Button variant='secondary' onClick={() => { setShowAddModal(false); setShowEditModal(false); }}>
                        Cancel
                    </Button>
                </Modal.Footer>
            </Modal>

            {/* Delete Confirmation Modal */}
            <Modal show={showDeleteModal} onHide={() => setShowDeleteModal(false)}>
                <Modal.Header closeButton>
                    <Modal.Title>Confirm Delete</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    Are you sure you want to delete the token &quot;{selectedToken?.id}&quot;? This action cannot be undone.
                </Modal.Body>
                <Modal.Footer>
                    <Button variant='danger' onClick={handleDeleteConfirm}>
                        Delete
                    </Button>
                    <Button variant='secondary' onClick={() => setShowDeleteModal(false)}>
                        Cancel
                    </Button>
                </Modal.Footer>
            </Modal>
        </Container>
    );
}

export default TokenManagement;
