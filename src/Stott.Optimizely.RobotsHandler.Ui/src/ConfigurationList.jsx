import { useState, useEffect } from 'react';
import axios from 'axios';
import { Alert, Container, Row } from 'react-bootstrap';
import AddSiteRobots from './AddSiteRobots';
import EditSiteRobots from './EditSiteRobots';
import DeleteSiteRobots from './DeleteSiteRobots';

function ConfigurationList(props)
{

    const [siteCollection, setSiteCollection] = useState([])

    useEffect(() => {
        getSiteCollection()
    }, [])

    const handleShowFailureToast = (title, description) => props.showToastNotificationEvent && props.showToastNotificationEvent(false, title, description)

    const getSiteCollection = async () => {
        
        setSiteCollection([]);
        
        await axios.get(import.meta.env.VITE_APP_ROBOTS_LIST)
            .then((response) => {
                if (response.data && response.data.list && Array.isArray(response.data.list)){
                    setSiteCollection(response.data.list);
                }
                else{
                    handleShowFailureToast('Failure', 'Failed to retrieve robots configuration data.');
                }
            },
            () => {
                handleShowFailureToast('Failure', 'Failed to retrieve robots configuration data.');
            });
    }

    const renderSiteList = () => {
        return siteCollection && siteCollection.map((siteDetails, index) => {
            const { id, siteId, siteName, isForWholeSite, specificHost, canDelete } = siteDetails
            const hostName = isForWholeSite === true ? 'Default' : specificHost;
            return (
                <tr key={index}>
                    <td>{siteName}</td>
                    <td>{hostName}</td>
                    <td>
                        <EditSiteRobots id={id} siteId={siteId} showToastNotificationEvent={props.showToastNotificationEvent} reloadEvent={getSiteCollection}></EditSiteRobots>
                        <DeleteSiteRobots id={id} siteName={siteName} showToastNotificationEvent={props.showToastNotificationEvent} canDelete={canDelete} reloadEvent={getSiteCollection}></DeleteSiteRobots>
                    </td>
                </tr>
            )
        })
    }

    return(
        <Container>
            <Row className='mb-2'>
                <div className='col-xl-9 col-lg-9 col-sm-12 col-xs-12 p-0'>
                    <Alert variant='primary' className='p-3'>A default configuration will always be shown for each site to reflect the fallback behaviour of the AddOn.</Alert>
                </div>
                <div className='col-xl-3 col-lg-3 col-sm-12 col-xs-12 p-0 text-end'>
                    <AddSiteRobots showToastNotificationEvent={props.showToastNotificationEvent} reloadEvent={getSiteCollection}></AddSiteRobots>
                </div>
            </Row>
            <Row>
                <table className='table table-striped'>
                    <thead>
                        <tr>
                            <th className='table-header-fix'>Site Name</th>
                            <th className='table-header-fix'>Host</th>
                            <th className='table-header-fix'>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {renderSiteList()}
                    </tbody>
                </table>
            </Row>
        </Container>
    )
}

export default ConfigurationList