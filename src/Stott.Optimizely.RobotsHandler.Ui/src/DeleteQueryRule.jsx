import { useState } from 'react';
import axios from 'axios';
import { Button, Modal } from 'react-bootstrap';

function DeleteQueryRule(props) {

    const [showModal, setShowModal] = useState(false);

    const handleCloseModal = () => {
        setShowModal(false);
    };

    const handleShowDeleteModal = () => {
        setShowModal(true);
    };

    const handleDeleteQueryRule = async () => {
        let url = ''.concat(import.meta.env.VITE_APP_QUERY_RULES_DELETE, props.id, '/');
        await axios.delete(url)
            .then(() => {
                handleShowSuccessToast('Success', 'Your query rule for \'' + props.queryName + '\' was successfully deleted.');
                setShowModal(false);
                handleReload();
            },
            () => {
                handleShowFailureToast('Failure', 'An error was encountered when trying to delete your query rule for \'' + props.queryName + '\'.');
                setShowModal(false);
            });
    };

    const handleShowSuccessToast = (title, description) => props.showToastNotificationEvent && props.showToastNotificationEvent(true, title, description);
    const handleShowFailureToast = (title, description) => props.showToastNotificationEvent && props.showToastNotificationEvent(false, title, description);
    const handleReload = () => props.reloadEvent && props.reloadEvent();

    return(
        <>
            <Button variant='danger' className='text-nowrap' onClick={handleShowDeleteModal}>Delete</Button>
            <Modal show={showModal}>
                <Modal.Header closeButton onClick={handleCloseModal}>
                    <Modal.Title>Delete Query Rule</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <label>Are you sure you want to delete this query rule for '{props.queryName}'?</label>
                </Modal.Body>
                <Modal.Footer>
                    <Button variant='danger' type='button' onClick={handleDeleteQueryRule}>Delete</Button>
                    <Button variant='secondary' type='button' onClick={handleCloseModal}>Cancel</Button>
                </Modal.Footer>
            </Modal>
        </>
    )
}

export default DeleteQueryRule;