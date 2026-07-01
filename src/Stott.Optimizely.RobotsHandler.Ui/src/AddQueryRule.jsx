import { useState, useEffect } from 'react'
import axios from 'axios';
import { Button, Modal } from 'react-bootstrap'

function AddQueryRule(props) {

    const [showModal, setShowModal] = useState(false)
    const [queryName, setQueryName] = useState('')
    const [matchRule, setMatchRule] = useState('')
    const [robotsValue, setRobotsValue] = useState('')
    const [isEnabled, setIsEnabled] = useState(true)

    const handleShowModal = () => {
        setQueryName('');
        setMatchRule('');
        setRobotsValue('');
        setIsEnabled(true);
        setShowModal(true);
    };

    const handleCloseModal = () => {
        setShowModal(false);
    };

    const handleSaveQueryRule = async () => {

        let params = new URLSearchParams();
        params.append('queryName', queryName);
        params.append('matchRule', matchRule);
        params.append('robotsValue', robotsValue);
        params.append('isEnabled', isEnabled);

        await axios.post(import.meta.env.VITE_APP_QUERY_RULES_SAVE, params)
            .then(() => {
                handleShowSuccessToast('Success', 'Your query rule for \'' + queryName + '\' was successfully applied.');
                setShowModal(false);
                handleReload();
            },
            (error) => {
                if (error.response && error.response.status === 409) {
                    handleShowFailureToast('Failure', error.response.data);
                    setShowModal(false);
                }
                else {
                    handleShowFailureToast('Failure', 'An error was encountered when trying to save your query rule.');
                    setShowModal(false);
                }
            });
    };

    const handleShowSuccessToast = (title, description) => props.showToastNotificationEvent && props.showToastNotificationEvent(true, title, description);
    const handleShowFailureToast = (title, description) => props.showToastNotificationEvent && props.showToastNotificationEvent(false, title, description);
    const handleReload = () => props.reloadEvent && props.reloadEvent();

    return(
        <>
            <Button variant='primary' onClick={handleShowModal} className='text-nowrap p-3'>Add Query Rule</Button>
            <Modal show={showModal} size='xl'>
                <Modal.Header closeButton onClick={handleCloseModal}>
                    <Modal.Title>Create Query Rule</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <div className='mb-3'>
                        <label>Query Name</label>
                        <input className='form-control' name='QueryName' type='text' onChange={(e) => setQueryName(e.target.value)} value={queryName}></input>
                    </div>
                    <div className='mb-3'>
                        <label>Match Rule</label>
                        <input className='form-control' name='MatchRule' type='text' onChange={(e) => setMatchRule(e.target.value)} value={matchRule}></input>
                    </div>
                    <div className='mb-3'>
                        <label>Robots Value</label>
                        <input className='form-control' name='RobotsValue' type='text' onChange={(e) => setRobotsValue(e.target.value)} value={robotsValue}></input>
                    </div>
                    <div className='mb-3'>
                        <label>Is Enabled</label>
                        <input className='form-control' name='IsEnabled' type='checkbox' onChange={(e) => setIsEnabled(e.target.checked)} checked={isEnabled}></input>
                    </div>
                </Modal.Body>
                <Modal.Footer>
                    <Button variant='primary' type='button' onClick={handleSaveQueryRule}>Save Changes</Button>
                    <Button variant='secondary' type='button' onClick={handleCloseModal}>Cancel</Button>
                </Modal.Footer>
            </Modal>
        </>
    )
}

export default AddQueryRule