import React, { useState } from 'react';
import axios from 'axios';
import { Alert, Button, Card, Form } from 'react-bootstrap';

function EnvironmentConfiguration(props) {
    
    const [useNoFollow, setUseNoFollow] = useState(props.environment.useNoFollow ?? false);
    const [useNoIndex, setUseNoIndex] = useState(props.environment.useNoIndex ?? false);
    const [useNoImageIndex, setUseNoImageIndex] = useState(props.environment.useNoImageIndex ?? false);
    const [useNoArchive, setUseNoArchive] = useState(props.environment.useNoArchive ?? false);
    const [enableSaveButton, setEnableSaveButton] = useState(false);
    const configurationId = props.environment.id ?? '00000000-0000-0000-0000-000000000000';
    const environmentName = props.environment.environmentName ?? 'Unknown'
    const isCurrentEnvironment = props.environment.isCurrentEnvironment ?? false;

    const handleSetUseNoFollow = (event) => { setUseNoFollow(event.target.checked); setEnableSaveButton(true); }
    const handleSetUseNoIndex = (event) => { setUseNoIndex(event.target.checked); setEnableSaveButton(true); }
    const handleSetUseNoImageIndex = (event) => { setUseNoImageIndex(event.target.checked); setEnableSaveButton(true); }
    const handleSetUseNoArchive = (event) => { setUseNoArchive(event.target.checked); setEnableSaveButton(true); }
    const getEnvironmentClass = () => { return isCurrentEnvironment ? 'fw-bold bg-primary text-light' : 'fw-bold'; }
    const getEnvironmentSuffix = () => { return isCurrentEnvironment ? ' (Current)' : ''; }
    
    const getInfoPanel = () => {
        let isActive = useNoArchive || useNoFollow || useNoIndex || useNoImageIndex;
        if (isCurrentEnvironment && isActive) {
            return (
                <Alert variant='warning' className='my-2 p-2'>
                    If a meta robots element is present within the HTML of the page, it will have its contents replaced by the options selected here.
                    These options will also be present on every response from the site in an <strong>X-Robots-Tag</strong> header.
                </Alert>
            )
        }
        else if (isActive) {
            return (
                <Alert variant='secondary' className='my-2 p-2'>
                    These options will be applied when the current database is cloned into to the <strong>{environmentName}</strong> environment.
                </Alert>
            )
        }
        else if (isCurrentEnvironment) {
            return (
                <p className='my-2 form-text'>
                    Please note that no action will be taken unless at least one option is selected.
                </p>
            )
        }
        else {
            return null;
        }
    }

    const handleSaveEnvironmentContent = async () => {
        let params = new URLSearchParams();
        params.append('id', configurationId);
        params.append('environmentName', environmentName);
        params.append('useNoFollow', useNoFollow);
        params.append('useNoIndex', useNoIndex);
        params.append('useNoImageIndex', useNoImageIndex);
        params.append('useNoArchive', useNoArchive);

        await axios.post(import.meta.env.VITE_APP_ENVIRONMENT_SAVE, params)
            .then(() => {
                handleShowSuccessToast('Success', 'Your environment configuration changes for \'' + environmentName + '\' were successfully applied.');
                setEnableSaveButton(false);
            },
            (error) => {
                if (error.response && error.response.status === 409) {
                    handleShowFailureToast('Failure', error.response.data);
                }
                else {
                    handleShowFailureToast('Failure', 'An error was encountered when trying to save your environment configuration for \'' + environmentName + '\'.');
                }
            });
    }

    const handleShowSuccessToast = (title, description) => props.showToastNotificationEvent && props.showToastNotificationEvent(true, title, description);
    const handleShowFailureToast = (title, description) => props.showToastNotificationEvent && props.showToastNotificationEvent(false, title, description);

    return(
        <Card className='my-3'>
            <Card.Header className={getEnvironmentClass()}>{environmentName} {getEnvironmentSuffix()}</Card.Header>
            <Card.Body>
                <Form.Group className='mb-3'>
                    <Form.Label>Select Instructions to Apply Globally to Robots</Form.Label>
                    <Form.Check type='switch' label='No Follow - Instruct search engines not to follow links on a page.' checked={useNoFollow} onChange={handleSetUseNoFollow} />
                    <Form.Check type='switch' label='No Index - Instruct search engines not to index a page.' checked={useNoIndex} onChange={handleSetUseNoIndex} />
                    <Form.Check type='switch' label='No Image Index - Instruct search engines not to index images on a page.' checked={useNoImageIndex} onChange={handleSetUseNoImageIndex} />
                    <Form.Check type='switch' label='No Archive - Instruct search engines not to show a cached link in search results.' checked={useNoArchive} onChange={handleSetUseNoArchive} />
                </Form.Group>
                {getInfoPanel()}
            </Card.Body>
            <Card.Footer>
                <Button disabled={!enableSaveButton} onClick={handleSaveEnvironmentContent}>Save Changes</Button>
            </Card.Footer>
        </Card>)
}

export default EnvironmentConfiguration
