import { useState } from 'react'
import axios from 'axios';
import { Alert, Button, Modal } from 'react-bootstrap'

function EditSiteRobots(props) {

    const [showModal, setShowModal] = useState(false)
    const [id, setId] = useState(props.id ?? '')
    const [siteId, setSiteId] = useState(props.siteId ?? '')
    const [siteName, setSiteName] = useState('')
    const [siteRobotsContent, setSiteRobotsContent] = useState('')
    const [availableHosts, setAvailableHosts] = useState([])
    const [isDefault, setIsDefault] = useState(true)
    const [specificHost, setSpecificHost] = useState('')

    const handleSiteRobotsContentChange = (event) => {
        setSiteRobotsContent(event.target.value);
    }

    const handleChangeHost = (event) => {
        setSpecificHost(event.target.value);
        setIsDefault(event.target.value === '');
    }

    const handleShowSuccessToast = (title, description) => props.showToastNotificationEvent && props.showToastNotificationEvent(true, title, description);
    const handleShowFailureToast = (title, description) => props.showToastNotificationEvent && props.showToastNotificationEvent(false, title, description);

    const handleShowEditModal = async () => {
        await axios.get(import.meta.env.VITE_APP_ROBOTS_EDIT, { params: { id: id, siteId: siteId } })
            .then((response) => {
                if (response.data) {
                    setId(response.data.id);
                    setSiteId(response.data.siteId);
                    setSiteName(response.data.siteName);
                    setSiteRobotsContent(response.data.robotsContent);
                    setAvailableHosts(response.data.availableHosts ?? []);
                    setIsDefault(response.data.isForWholeSite ?? true);
                    setSpecificHost(response.data.specificHost ?? '');
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
        params.append('id', id);
        params.append('siteId', siteId);
        params.append('siteName', siteName);
        params.append('specificHost', specificHost);
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

    const renderAvailableHosts = () => {
        return availableHosts && availableHosts.map(host => {
            const { key, value } = host
            const isSelected = value === specificHost;
            return (
                <option value={value} selected={isSelected}>{key}</option>
            )
        })
    }

    return (
        <>
            <Button variant='primary' onClick={handleShowEditModal} className='text-nowrap'>Edit</Button>
            <Modal show={showModal} size='lg'>
                <Modal.Header closeButton onClick={handleCloseModal}>
                    <Modal.Title>{siteName}</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <div className='mb-3'>
                        <label>Host</label>
                        <select className='form-control form-select' name='SpecificHost' onChange={handleChangeHost}>{renderAvailableHosts()}</select>
                    </div>
                    <Alert variant='primary' show={isDefault} className='my-2 p-2'>
                        Please note that robots content for a host of 'Default' will be used where robots content has not been set for a specific host.
                    </Alert>
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
