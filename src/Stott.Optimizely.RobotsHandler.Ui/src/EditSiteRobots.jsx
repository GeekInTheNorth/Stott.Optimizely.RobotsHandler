import { useState } from 'react'
import axios from 'axios';
import { Button, Modal } from 'react-bootstrap'

function EditSiteRobots(props) {

    const [showModal, setShowModal] = useState(false)
    const [siteId, setSiteId] = useState(props.siteId ?? '')
    const [siteName, setSiteName] = useState('')
    const [siteRobotsContent, setSiteRobotsContent] = useState('')

    const handleSiteRobotsContentChange = (event) => {
        setSiteRobotsContent(event.target.value);
    }

    const handleShowSuccessToast = (title, description) => props.showToastNotificationEvent && props.showToastNotificationEvent(true, title, description);
    const handleShowFailureToast = (title, description) => props.showToastNotificationEvent && props.showToastNotificationEvent(false, title, description);

    const handleShowEditModal = async () => {
        await axios.get(import.meta.env.VITE_APP_ROBOTS_EDIT, { params: { siteId: siteId } })
            .then((response) => {
                if (response.data) {
                    setSiteName(response.data.siteName);
                    setSiteRobotsContent(response.data.robotsContent);
                    setShowModal(true);
                }
                else{
                    handleShowFailureToast('Failure', 'An error was encountered when trying to retrieve your robots.txt content.');
                }
            },
            () => {
                handleShowFailureToast('Failure', 'An error was encountered when trying to retrieve your robots.txt content.');
            });
    }

    const handleSaveRobotsContent = async () => {
        let params = new URLSearchParams();
        params.append('siteId', siteId);
        params.append('siteName', siteName);
        params.append('robotsContent', siteRobotsContent);

        await axios.post(import.meta.env.VITE_APP_ROBOTS_SAVE, params)
            .then(() => {
                handleShowSuccessToast('Success', 'Your robots.txt content changes for \'' + siteName + '\' were successfully applied.');
                setShowModal(false);
            },
            () => {
                handleShowFailureToast('Failure', 'An error was encountered when trying to save your robots.txt content for \'' + siteName + '\'.');
                setShowModal(false);
            });
    }

    const handleCloseModal = () => {
        setShowModal(false);
    }

    return (
        <>
            <Button variant='primary' onClick={handleShowEditModal} className='text-nowrap'>Edit</Button>
            <Modal show={showModal}>
                <Modal.Header closeButton onClick={handleCloseModal}>
                    <Modal.Title>{siteName}</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <div className='mb-3'>
                        <label>Robots.txt Content</label>
                        <textarea className='form-control' name='RobotsContent' cols='60' rows='10' onChange={handleSiteRobotsContentChange} value={siteRobotsContent}></textarea>
                    </div>
                </Modal.Body>
                <Modal.Footer>
                    <Button variant='primary' type='button' onClick={handleSaveRobotsContent}>Save Changes</Button>
                    <Button variant='secondary' type='button' onClick={handleCloseModal}>Cancel</Button>
                </Modal.Footer>
            </Modal>
        </>
    )
}

export default EditSiteRobots
