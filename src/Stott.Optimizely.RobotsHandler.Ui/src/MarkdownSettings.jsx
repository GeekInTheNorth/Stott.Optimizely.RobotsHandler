import { useState, useEffect } from 'react';
import axios from 'axios';
import PropTypes from 'prop-types';
import { Button, Card } from 'react-bootstrap';

function MarkdownSettings({ showToastNotificationEvent })
{
    const [isEnabled, setIsEnabled] = useState(false);
    const [userAgents, setUserAgents] = useState([]);

    useEffect(() => {
        const fetchMarkdownSettings = async () => {
            try {
                const response = await axios.get(import.meta.env.VITE_APP_MARKDOWNMAPPING_SETTINGS);
                setIsEnabled(response.data.isEnabled);
                setUserAgents(response.data.userAgents);
            } catch {
                showToastNotificationEvent && showToastNotificationEvent(false, 'Failure', 'Failed to retrieve markdown settings.');
            }
        };

        fetchMarkdownSettings();
    }, []);

    return (
        <div className='container my-4'>
        <Card>
            <Card.Header>Markdown Settings</Card.Header>
            <Card.Body>
                <p><strong>Is Enabled:</strong> {isEnabled ? 'Yes' : 'No'}</p>
                <p>
                    <strong>User Agents:</strong>&nbsp;
                    {userAgents.map((agent, index) => (
                        <>
                            {index > 0 && ', '}
                            <span key={index}>{agent}</span>
                        </>
                    ))}
                </p>
            </Card.Body>
            <Card.Footer className='text-end'>
                <Button variant="primary">Edit Settings</Button>
            </Card.Footer>
        </Card>
        </div>
    );
}

MarkdownSettings.propTypes = {
    showToastNotificationEvent: PropTypes.func.isRequired
};

export default MarkdownSettings;