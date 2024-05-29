import { useState, useEffect } from 'react'
import axios from 'axios';
import { Container } from 'react-bootstrap'
import EditSiteRobots from './EditSiteRobots';

function ConfigurationList(props)
{

    const [siteCollection, setSiteCollection] = useState([])

    useEffect(() => {
        getSiteCollection()
    }, [])

    const handleShowFailureToast = (title, description) => props.showToastNotificationEvent && props.showToastNotificationEvent(false, title, description)

    const getSiteCollection = async () => {
        await axios.get(import.meta.env.VITE_APP_ROBOTS_LIST)
            .then((response) => {
                if (response.data && response.data.list && Array.isArray(response.data.list)){
                    setSiteCollection(response.data.list);
                }
                else{
                    handleShowFailureToast('Failure', 'Failed to retrieve site data.');
                }
            },
            () => {
                handleShowFailureToast('Failure', 'Failed to retrieve site data.');
            });
    }

    const renderSiteList = () => {
        return siteCollection && siteCollection.map(siteDetails => {
            const { id, siteId, siteName, availableHosts, isForWholeSite, specificHost, robotsContent } = siteDetails
            const hostName = isForWholeSite === true ? 'All Hosts' : specificHost;
            return (
                <tr key={id}>
                    <td>{siteName}</td>
                    <td>{hostName}</td>
                    <td>
                        <EditSiteRobots siteId={id} showToastNotificationEvent={props.showToastNotificationEvent}></EditSiteRobots>
                    </td>
                </tr>
            )
        })
    }

    return(
        <Container>
            <table className='table table-striped'>
                <thead>
                    <tr>
                        <th className='table-header-fix'>Site Name</th>
                        <th className='table-header-fix'>Hosts</th>
                        <th className='table-header-fix'>Actions</th>
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