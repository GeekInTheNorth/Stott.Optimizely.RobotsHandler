import { useState } from 'react'
import axios from 'axios';
import { Button, Form, Modal } from 'react-bootstrap'
import MatchRuleField from './MatchRuleField'
import RobotsField from './RobotsField'

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
                    <MatchRuleField value={matchRule} onChange={setMatchRule} />
                    <RobotsField value={robotsValue} onChange={setRobotsValue} />
                    <div className='mb-3'>
                        <Form.Check type='checkbox' name='IsEnabled' label='Is Enabled' checked={isEnabled} onChange={(e) => setIsEnabled(e.target.checked)} />
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
