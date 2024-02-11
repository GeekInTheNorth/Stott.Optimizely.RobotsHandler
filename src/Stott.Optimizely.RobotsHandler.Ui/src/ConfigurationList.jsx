import { useState, useEffect } from 'react'
import axios from 'axios';
import { Container } from 'react-bootstrap'

function ConfigurationList()
{

    const [siteCollection, setSiteCollection] = useState([])

    useEffect(() => {
        getSiteCollection()
    }, [])

    const getSiteCollection = async () => {
        console.log(import.meta.env.VITE_APP_ROBOTS_LIST);
        await axios.get(import.meta.env.VITE_APP_ROBOTS_LIST)
            .then((response) => {
                if (response.data && response.data.list && Array.isArray(response.data.list)){
                    setSiteCollection(response.data.list);
                }
                else{
                    // handleShowFailureToast("Get CSP Sources", "Failed to retrieve Content Security Policy Sources.");
                }
            },
            () => {
                // handleShowFailureToast("Error", "Failed to retrieve the Content Security Policy Sources.");
            });
    }

    const renderSiteList = () => {
        return siteCollection && siteCollection.map((siteDetails, index) => {
            const { id, name, url } = siteDetails
            return (
                <tr key={id}>
                    <td>{name}</td>
                    <td>{url}</td>
                    <td></td>
                </tr>
            )
        })
    }

    return(
        <Container>
            <table className='table table-striped'>
                <thead>
                    <tr>
                        <th>Site Name</th>
                        <th>Host Url</th>
                        <th>Actions</th>
                    </tr>
                </thead>
                <tbody>
                    {renderSiteList()}
                </tbody>
            </table>
        </Container>
    )
}

export default ConfigurationList