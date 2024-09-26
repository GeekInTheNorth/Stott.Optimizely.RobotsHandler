import React, { useState, useEffect } from 'react';
import { Alert, Card, Container, Form, Row } from 'react-bootstrap';

function EnvironmentConfiguration(props) {
    
    const [robotsTagBehavior, setRobotsTagBehavior] = useState(false);
    const [useNoFollow, setUseNoFollow] = useState(false);
    const [useNoIndex, setUseNoIndex] = useState(false);
    const [useNoImageIndex, setUseNoImageIndex] = useState(false);
    const [useNoArchive, setUseNoArchive] = useState(false);
    const [useNoSnippet, setUseNoSnippet] = useState(false);
    const [useNoTranslate, setUseNoTranslate] = useState(false);
    const environmentName = props.environmentName ?? 'Unknown'

    const handleSetUseNoFollow = (event) => { setUseNoFollow(event.target.checked); }
    const handleSetUseNoIndex = (event) => { setUseNoIndex(event.target.checked); }
    const handleSetUseNoImageIndex = (event) => { setUseNoImageIndex(event.target.checked); }
    const handleSetUseNoArchive = (event) => { setUseNoArchive(event.target.checked); }
    const handleSetUseNoSnippet = (event) => { setUseNoSnippet(event.target.checked); }
    const handleSetUseNoTranslate = (event) => { setUseNoTranslate(event.target.checked); }
    const handleSetRobotsTagBehavior = (event) => { setRobotsTagBehavior(event.target.value); }

    return(
        <Card className='my-3'>
            <Card.Header className='fw-bold'>{environmentName}</Card.Header>
            <Card.Body>
                <Form.Group className='mb-3'>
                    <Form.Label id='lblConfigurationType'>Robot Tag Behaviour</Form.Label>
                    <Form.Select label='Robot Tag Behaviour' aria-describedby='lblConfigurationType' onChange={handleSetRobotsTagBehavior} value={robotsTagBehavior}>
                        <option value='None'>Disabled</option>
                        <option value='Replace'>Replace Page Values</option>
                        <option value='Merge'>Merge With Page Values</option>
                    </Form.Select>
                </Form.Group>
                <Form.Group className='mb-3'>
                    <Form.Label>Robots Options</Form.Label>
                    <Form.Check type='switch' label='No Follow - Instruct search engines not to follow links on a page.' checked={useNoFollow} onChange={handleSetUseNoFollow} />
                    <Form.Check type='switch' label='No Index - Instruct search engines not to index a page.' checked={useNoIndex} onChange={handleSetUseNoIndex} />
                    <Form.Check type='switch' label='No Image Index - Instruct search engines not to index images on a page.' checked={useNoImageIndex} onChange={handleSetUseNoImageIndex} />
                    <Form.Check type='switch' label='No Archive - Instruct search engines not to show a cached link in search results.' checked={useNoArchive} onChange={handleSetUseNoArchive} />
                    <Form.Check type='switch' label='No Snippet - Instruct search engines not to show a text snippet or video preview in the search results.' checked={useNoSnippet} onChange={handleSetUseNoSnippet} />
                    <Form.Check type='switch' label='No Translate - Instruct search engines not to offer a translation of a page in search results.' checked={useNoTranslate} onChange={handleSetUseNoTranslate} />
                </Form.Group>
            </Card.Body>
        </Card>)
}

export default EnvironmentConfiguration
