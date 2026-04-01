import { useState, useEffect } from 'react'
import axios from 'axios';
import { Alert, Button, Modal } from 'react-bootstrap'

function AddSiteLlms(props) {

    const [showModal, setShowModal] = useState(false);
    const [appCollection, setAppCollection] = useState([]);
    const [hostCollection, setHostCollection] = useState([]);
    const [appId, setAppId] = useState(null);
    const [appName, setAppName] = useState(null);
    const [siteLlmsContent, setSiteLlmsContent] = useState('');
    const [hostName, setHostName] = useState('');
    const [isDefault, setIsDefault] = useState(true)

    const handleCloseModal = () => {
        setShowModal(false);
    }

    const getDefaultLlmsContent = () => {
        return `# Title

> Optional description goes here

Optional details go here

## Section name

- [Link title](https://link_url): Optional link details

## Optional

- [Link title](https://link_url)`;
    };

    const handleShowEditModal = async () => {
        await axios.get(import.meta.env.VITE_APP_APPLICATIONS_LIST)
            .then((response) => {
                if (response.data && response.data && Array.isArray(response.data)){
                    setAppCollection(response.data);
                    if(response.data.length > 0){
                        var firstApp = response.data[0];
                        var hosts = firstApp.availableHosts ?? [];
                        setAppId(firstApp.appId);
                        setAppName(firstApp.appName);
                        setHostCollection(hosts);
                        if (hosts.length > 0){
                            setHostName(hosts[0].hostName);
                        }
                    }

                    setSiteLlmsContent(getDefaultLlmsContent());
                    setShowModal(true);
                }
                else{
                    handleShowFailureToast('Failure', 'Failed to retrieve application data.');
                }
            },
            () => {
                handleShowFailureToast('Failure', 'Failed to retrieve application data.');
            });
    }

    const handleSaveLlmsContent = async () => {

        let selectedApp = getSelectedApp();
        let selectedHost = getSelectedHostName();
        let params = new URLSearchParams();
        params.append('appId', selectedApp.appId);
        params.append('appName', selectedApp.appName);
        params.append('specificHost', selectedHost);
        params.append('llmsContent', siteLlmsContent);

        await axios.post(import.meta.env.VITE_APP_LLMS_SAVE, params)
            .then(() => {
                handleShowSuccessToast('Success', 'Your llms.txt content changes for \'' + appName + '\' were successfully applied.');
                setShowModal(false);
                handleReload();
            },
            (error) => {
                if (error.response && error.response.status === 409) {
                    handleShowFailureToast('Failure', error.response.data);
                    setShowModal(false);
                }
                else {
                    handleShowFailureToast('Failure', 'An error was encountered when trying to save your llms.txt content.');
                    setShowModal(false);
                }
            });
    }

    const handleAppSelection = (event) => {
        const selectedAppId = event.target.value;
        const selectedApp = appCollection.filter(x => x.appId == selectedAppId)[0];
        const availableHosts = selectedApp.availableHosts ?? [];
        const firstHost = availableHosts.length > 0 ? availableHosts[0].hostName : '';

        setAppId(selectedApp.appId);
        setAppName(selectedApp.appName);
        setHostName(firstHost);
        setHostCollection(availableHosts);
    }

    const handleHostSelection = (event) => {
        let selectedHost = event.target.value ?? '';
        setHostName(selectedHost);
        setIsDefault(selectedHost === '');
    }

    const handleSiteLlmsContentChange = (event) => {
        setSiteLlmsContent(event.target.value);
    }

    const renderAvailableApps = () => {
        return appCollection && appCollection.map((app, index) => {
            const { appId, appName } = app
            return (
                <option key={index} value={appId}>{appName}</option>
            )
        })
    }

    const renderAvailableHosts = () => {
        return hostCollection && hostCollection.map((host, index) => {
            const { hostName, displayName } = host
            return (
                <option key={index} value={hostName}>{displayName}</option>
            )
        })
    }

    const getSelectedApp = () => {
        if (appId === undefined || appId === null || appId === '') {
            var firstApp = appCollection[0];
            setAppId(firstApp.appId);
            setAppName(firstApp.appName);

            return firstApp;
        }

        var matches = appCollection.filter(matchApp);

        return matches[0];
    }

    const matchApp = (thisApp) => {
        return thisApp && thisApp.appId && thisApp.appId === appId;
    }

    const getSelectedHostName = () => {
        if (hostName === undefined || hostName === null || hostCollection.length === 0){
            return '';
        }

        return hostName;
    }

    useEffect(() => { renderAvailableHosts() }, [hostCollection]);

    const handleShowSuccessToast = (title, description) => props.showToastNotificationEvent && props.showToastNotificationEvent(true, title, description);
    const handleShowFailureToast = (title, description) => props.showToastNotificationEvent && props.showToastNotificationEvent(false, title, description);
    const handleReload = () => props.reloadEvent && props.reloadEvent();

    return(
        <>
            <Button variant='primary' onClick={handleShowEditModal} className='text-nowrap p-3'>Add Configuration</Button>
            <Modal show={showModal} size='xl'>
                <Modal.Header closeButton onClick={handleCloseModal}>
                    <Modal.Title>Create LLMS Configuration</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                <div className='mb-3'>
                        <label>Application</label>
                        <select className='form-control form-select' name='SpecificHost' onChange={handleAppSelection}>{renderAvailableApps()}</select>
                    </div>
                    <div className='mb-3'>
                        <label>Host</label>
                        <select className='form-control form-select' name='SpecificHost' value={hostName} onChange={handleHostSelection}>{renderAvailableHosts()}</select>
                    </div>
                    <Alert variant='primary' show={isDefault} className='my-2 p-2'>
                        Please note that LLMS content for a host of 'Default' will be used where LLMS content has not been set for a specific host.
                    </Alert>
                    <div className='mb-3'>
                        <label>LLMS.txt Content</label>
                        <textarea className='form-control large-text-area' name='LlmsContent' cols='60' rows='10' onChange={handleSiteLlmsContentChange} value={siteLlmsContent}>

                        </textarea>
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

export default AddSiteLlms;