import { useState } from 'react'
import axios from 'axios';
import { Alert, Button, Modal } from 'react-bootstrap'

function EditSiteLlms(props) {

    const [showModal, setShowModal] = useState(false)
    const [id, setId] = useState(props.id ?? '')
    const [siteId, setSiteId] = useState(props.siteId ?? '')
    const [siteName, setSiteName] = useState('')
    const [siteLlmsContent, setSiteLlmsContent] = useState('')
    const [availableHosts, setAvailableHosts] = useState([])
    const [isDefault, setIsDefault] = useState(true)
    const [specificHost, setSpecificHost] = useState('')

    const handleSiteLlmsContentChange = (event) => {
        setSiteLlmsContent(event.target.value);
    }

    const handleChangeHost = (event) => {
        setSpecificHost(event.target.value);
        setIsDefault(event.target.value === '');
    }

    const handleShowEditModal = async () => {
        await axios.get(import.meta.env.VITE_APP_LLMS_EDIT, { params: { id: id, siteId: siteId } })
            .then((response) => {
                if (response.data) {
                    setId(response.data.id);
                    setSiteId(response.data.siteId);
                    setSiteName(response.data.siteName);
                    setSiteLlmsContent(response.data.llmsContent);
                    setAvailableHosts(response.data.availableHosts ?? []);
                    setIsDefault(response.data.isForWholeSite ?? true);
                    setSpecificHost(response.data.specificHost ?? '');
                    setShowModal(true);
                }
                else{
                    handleShowFailureToast('Failure', 'An error was encountered when trying to retrieve your llms.txt content.');
                }
            },
            () => {
                handleShowFailureToast('Failure', 'An error was encountered when trying to retrieve your llms.txt content.');
            });
    }

    const handleSaveLlmsContent = async () => {
        let params = new URLSearchParams();
        params.append('id', id);
        params.append('siteId', siteId);
        params.append('siteName', siteName);
        params.append('specificHost', specificHost);
        params.append('llmsContent', siteLlmsContent);

        await axios.post(import.meta.env.VITE_APP_LLMS_SAVE, params)
            .then(() => {
                handleShowSuccessToast('Success', 'Your llms.txt content changes for \'' + siteName + '\' were successfully applied.');
                setShowModal(false);
                handleReload();
            },
            (error) => {
                if (error.response && error.response.status === 409) {
                    handleShowFailureToast('Failure', error.response.data);
                    setShowModal(false);
                }
                else {
                    handleShowFailureToast('Failure', 'An error was encountered when trying to save your llms.txt content for \'' + siteName + '\'.');
                    setShowModal(false);
                }
            });
    }

    const handleCloseModal = () => {
        setShowModal(false);
    }

    const renderAvailableHosts = () => {
        return availableHosts && availableHosts.map(host => {
            const { hostName, displayName } = host
            const isSelected = hostName === specificHost;
            return (
                <option value={hostName} selected={isSelected}>{displayName}</option>
            )
        })
    }

    const handleShowSuccessToast = (title, description) => props.showToastNotificationEvent && props.showToastNotificationEvent(true, title, description);
    const handleShowFailureToast = (title, description) => props.showToastNotificationEvent && props.showToastNotificationEvent(false, title, description);
    const handleReload = () => props.reloadEvent && props.reloadEvent();

    return (
        <>
            <Button variant='primary' onClick={handleShowEditModal} className='text-nowrap me-2'>Edit</Button>
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
                        Please note that LLMS content for a host of 'Default' will be used where LLMS content has not been set for a specific host.
                    </Alert>
                    <div className='mb-3'>
                        <label>LLMS Content</label>
                        <textarea className='form-control' name='LlmsContent' cols='60' rows='10' onChange={handleSiteLlmsContentChange} value={siteLlmsContent}></textarea>
                    </div>
                </Modal.Body>
                <Modal.Footer>
                    <Button variant='primary' type='button' onClick={handleSaveLlmsContent}>Save Changes</Button>
                    <Button variant='secondary' type='button' onClick={handleCloseModal}>Cancel</Button>
                </Modal.Footer>
            </Modal>
        </>
    )
}

export default EditSiteLlms;