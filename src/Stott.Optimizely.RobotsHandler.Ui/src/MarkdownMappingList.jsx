import { useState, useEffect } from 'react';
import axios from 'axios';
import { Button, Container, Row, Table } from 'react-bootstrap';

function MarkdownMappingList(props)
{
    const [markdownMappings, setMarkdownMappings] = useState([]);

    useEffect(() => {
        const fetchMarkdownMappings = async () => {
            try {
                const response = await axios.get(import.meta.env.VITE_APP_MARKDOWNMAPPING_LIST);
                setMarkdownMappings(response.data);
            } catch {
                handleShowFailureToast('Failure', 'Failed to retrieve markdown mapping data.');
            }
        };

        fetchMarkdownMappings();
    }, []);

    const handleShowSuccessToast = (title, description) => props.showToastNotificationEvent && props.showToastNotificationEvent(true, title, description);
    const handleShowFailureToast = (title, description) => props.showToastNotificationEvent && props.showToastNotificationEvent(false, title, description);

    return (
        <Container>
            <table className='table table-striped'>
                <thead>
                    <tr>
                        <th>Display Name</th>
                        <th>Description</th>
                        <th>Content Type</th>
                        <th>Is Configured</th>
                        <th>Is Active</th>
                        <th>Actions</th>
                    </tr>
                </thead> 
                <tbody>
                    {markdownMappings.map(mapping => (
                        <tr key={mapping.id}>
                            <td className='align-middle'>{mapping.displayName}<br/><small>({mapping.contentName})</small></td>
                            <td className='align-middle'>{mapping.description}</td>
                            <td className='align-middle'>{mapping.contentType}</td>
                            <td className='align-middle'>{mapping.isConfigured ? 'Yes' : 'No'}</td>
                            <td className='align-middle'>{mapping.isActive ? 'Yes' : 'No'}</td>
                            <td className='align-middle'>
                                <Button variant="primary">Edit</Button>
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </Container>
    );
}

export default MarkdownMappingList;