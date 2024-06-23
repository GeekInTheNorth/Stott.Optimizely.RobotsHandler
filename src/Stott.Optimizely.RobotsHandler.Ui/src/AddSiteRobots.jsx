import { useState, useEffect } from 'react'
import axios from 'axios';
import { Button, Modal } from 'react-bootstrap'

function AddSiteRobots(props) {

    const [showModal, setShowModal] = useState(false)
    const [siteCollection, setSiteCollection] = useState([])
    const [hostCollection, setHostCollection] = useState([])
    const [siteId, setSiteId] = useState(null);
    const [siteName, setSiteName] = useState(null);
    const [siteRobotsContent, setSiteRobotsContent] = useState('')
    const [hostName, setHostName] = useState('');

    const handleCloseModal = () => {
        setShowModal(false);
    }

    const handleShowEditModal = async () => {
        await axios.get(import.meta.env.VITE_APP_SITES_LIST)
            .then((response) => {
                if (response.data && response.data && Array.isArray(response.data)){
                    setSiteCollection(response.data);
                    if(response.data.length > 0){
                        var firstSite = response.data[0];
                        var hosts = firstSite.availableHosts ?? [];
                        setSiteId(firstSite.siteId);
                        setSiteName(firstSite.siteName);
                        setHostCollection(hosts);
                        if (hosts.length > 0){
                            setHostName(hosts[0].value);
                        }
                    }

                    setSiteRobotsContent('');
                    setShowModal(true);
                }
                else{
                    handleShowFailureToast('Failure', 'Failed to retrieve site data.');
                }
            },
            () => {
                handleShowFailureToast('Failure', 'Failed to retrieve site data.');
            });
    }

    const handleSaveRobotsContent = async () => {

        let selectedSite = getSelectedSite();
        let selectedHost = getSelectedHostName();
        let params = new URLSearchParams();
        params.append('siteId', selectedSite.siteId);
        params.append('siteName', selectedSite.siteName);
        params.append('specificHost', selectedHost);
        params.append('robotsContent', siteRobotsContent);

        await axios.post(import.meta.env.VITE_APP_ROBOTS_SAVE, params)
            .then(() => {
                handleShowSuccessToast('Success', 'Your robots.txt content changes for \'' + siteName + '\' were successfully applied.');
                setShowModal(false);
                handleReload();
            },
            (error) => {
                if (error.response && error.response.status === 409) {
                    handleShowFailureToast('Failure', error.response.data);
                    setShowModal(false);
                }
                else {
                    handleShowFailureToast('Failure', 'An error was encountered when trying to save your robots.txt content.');
                    setShowModal(false);
                }
            });
    }

    const handleSiteSelection = (event) => {
        const selectedSiteid = event.target.value;
        const selectedSite = siteCollection.filter(x => x.siteId == selectedSiteid)[0];
        const availableHosts = selectedSite.availableHosts ?? [];
        const firstHost = availableHosts.length > 0 ? availableHosts[0].value : '';

        setSiteId(selectedSite.siteId);
        setSiteName(selectedSite.siteName);
        setHostName(firstHost);
        setHostCollection(availableHosts);
    }

    const handleHostSelection = (event) => {
        setHostName(event.target.value ?? '');
    }

    const handleSiteRobotsContentChange = (event) => {
        setSiteRobotsContent(event.target.value);
    }

    const renderAvailableSites = () => {
        return siteCollection && siteCollection.map((site, index) => {
            const { siteId, siteName } = site
            return (
                <option key={index} value={siteId}>{siteName}</option>
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

    const getSelectedSite = () => {
        if (siteId === undefined || siteId === null || siteId === '') {
            var firstSite = siteCollection[0];
            setSiteId(firstSite.siteId);
            setSiteName(firstSite.siteName);

            return firstSite;
        }

        var matches = siteCollection.filter(matchSite);

        return matches[0];
    }

    const matchSite = (thisSite) => {
        return thisSite && thisSite.siteId && thisSite.siteId === siteId;
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
            <Modal show={showModal} size='lg'>
                <Modal.Header closeButton onClick={handleCloseModal}>
                    <Modal.Title>Create Robots Configuration</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                <div className='mb-3'>
                        <label>Site</label>
                        <select className='form-control form-select' name='SpecificHost' onChange={handleSiteSelection}>{renderAvailableSites()}</select>
                    </div>
                    <div className='mb-3'>
                        <label>Host</label>
                        <select className='form-control form-select' name='SpecificHost' value={hostName} onChange={handleHostSelection}>{renderAvailableHosts()}</select>
                    </div>
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

export default AddSiteRobots