import React, { useState } from 'react';
import { Alert, Button, Card, Form } from 'react-bootstrap';

function EnvironmentConfiguration(props) {
    
    const [useNoFollow, setUseNoFollow] = useState(props.environment.useNoFollow ?? false);
    const [useNoIndex, setUseNoIndex] = useState(props.environment.useNoIndex ?? false);
    const [useNoImageIndex, setUseNoImageIndex] = useState(props.environment.useNoImageIndex ?? false);
    const [useNoArchive, setUseNoArchive] = useState(props.environment.useNoArchive ?? false);
    const [enableSaveButton, setEnableSaveButton] = useState(false);
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
                <Button disabled={!enableSaveButton}>Save Changes</Button>
            </Card.Footer>
        </Card>)
}

export default EnvironmentConfiguration
