import { useState, useEffect } from 'react';
import axios from 'axios';
import { Alert, Container, Row } from 'react-bootstrap';
import EditSiteLlms from './EditSiteLlms';
import DeleteSiteLlms from './DeleteSiteLlms';
import AddSiteLlms from './AddSiteLlms';

function LlmsConfigurationList(props)
{

    const [appCollection, setAppCollection] = useState([])

    useEffect(() => {
        getAppCollection()
    }, [])

    const handleShowFailureToast = (title, description) => props.showToastNotificationEvent && props.showToastNotificationEvent(false, title, description)

    const getAppCollection = async () => {

        setAppCollection([]);

        await axios.get(import.meta.env.VITE_APP_LLMS_LIST)
            .then((response) => {
                if (response.data && response.data.list && Array.isArray(response.data.list)){
                    setAppCollection(response.data.list);
                }
                else{
                    handleShowFailureToast('Failure', 'Failed to retrieve llms configuration data.');
                }
            },
            () => {
                handleShowFailureToast('Failure', 'Failed to retrieve llms configuration data.');
            });
    }

    const renderAppList = () => {
        return appCollection && appCollection.map((appDetails, index) => {
            const { id, appId, appName, isForWholeSite, specificHost } = appDetails
            const hostName = isForWholeSite === true ? 'Default' : specificHost;
            return (
                <tr key={index}>
                    <td>{appName}</td>
                    <td>{hostName}</td>
                    <td>
                        <EditSiteLlms id={id} appId={appId} showToastNotificationEvent={props.showToastNotificationEvent} reloadEvent={getAppCollection}></EditSiteLlms>
                        <DeleteSiteLlms id={id} appName={appName} showToastNotificationEvent={props.showToastNotificationEvent} reloadEvent={getAppCollection}></DeleteSiteLlms>
                    </td>
                </tr>
            )
        })
    }

    return(
        <Container className='mt-3'>
            <Row className='mb-2'>
                <div className='col-xl-9 col-lg-9 col-sm-12 col-xs-12 p-0'>
                    <Alert variant='primary' className='p-3'>An llms.txt file should be written in <a href='https://www.markdownguide.org/basic-syntax/' target='_blank' rel='noopener noreferrer'>markdown</a> as it is human-readable and helps AI (like Large Language Models) understand a website's content and purpose. You can learn more <a href='https://llmstxt.org/' target='_blank' rel='noopener noreferrer'>here</a>.</Alert>
                </div>
                <div className='col-xl-3 col-lg-3 col-sm-12 col-xs-12 p-0 text-end'>
                    <AddSiteLlms showToastNotificationEvent={props.showToastNotificationEvent} reloadEvent={getAppCollection}></AddSiteLlms>
                </div>
            </Row>
            <Row>
                <table className='table table-striped'>
                    <thead>
                        <tr>
                            <th className='table-header-fix'>Application</th>
                            <th className='table-header-fix'>Host</th>
                            <th className='table-header-fix'>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {renderAppList()}
                    </tbody>
                </table>
            </Row>
        </Container>
    )
}

export default LlmsConfigurationList