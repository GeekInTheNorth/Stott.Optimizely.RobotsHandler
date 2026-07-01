import { useState, useEffect } from 'react';
import axios from 'axios';
import EditQueryRule from './EditQueryRule';
import DeleteQueryRule from './DeleteQueryRule';
import AddQueryRule from './AddQueryRule';
import { Container, Row } from 'react-bootstrap';

function QueryRulesList(props)
{
    const [rulesCollection, setRulesCollection] = useState([]);

    useEffect(() => {
        getRulesCollection()
    }, []);

    const getRulesCollection = async () => {

        setRulesCollection([]);

        await axios.get(import.meta.env.VITE_APP_QUERY_RULES_LIST)
            .then((response) => {
                if (response.data && response.data.list && Array.isArray(response.data.list)){
                    setRulesCollection(response.data.list);
                }
                else{
                    handleShowFailureToast('Failure', 'Failed to retrieve query rules configuration data.');
                }
            },
            () => {
                handleShowFailureToast('Failure', 'Failed to retrieve query rules configuration data.');
            });
    };

    const handleShowFailureToast = (title, description) => props.showToastNotificationEvent && props.showToastNotificationEvent(false, title, description);
    const handleShowSuccessToast = (title, description) => props.showToastNotificationEvent && props.showToastNotificationEvent(true, title, description);

    const renderRulesList = () => {
        return rulesCollection && rulesCollection.map((ruleDetails, index) => {
            const { id, queryName, matchRule, robotsValue, isEnabled } = ruleDetails;
            return (
                <tr key={id}>
                    <td>{queryName}</td>
                    <td>{matchRule}</td>
                    <td>{robotsValue}</td>
                    <td>{isEnabled ? 'Yes' : 'No'}</td>
                    <td>
                        <EditQueryRule id={id} queryName={queryName} showToastNotificationEvent={props.showToastNotificationEvent} reloadEvent={getRulesCollection}></EditQueryRule>
                        <DeleteQueryRule id={id} queryName={queryName} showToastNotificationEvent={props.showToastNotificationEvent} reloadEvent={getRulesCollection}></DeleteQueryRule>
                    </td>
                </tr>
            )
        })
    };

    return(
        <Container className='mt-3' fluid='xxl'>
            <Row className='mb-2'>
                <div className='col-12 p-0 text-end'>
                    <AddQueryRule showToastNotificationEvent={props.showToastNotificationEvent} reloadEvent={getRulesCollection}></AddQueryRule>
                </div>
            </Row>
            <Row>
                <table className='table table-striped'>
                    <thead>
                        <tr>
                            <th className='table-header-fix'>Query Name</th>
                            <th className='table-header-fix'>Match Rule</th>
                            <th className='table-header-fix'>Robots Value</th>
                            <th className='table-header-fix'>Is Enabled</th>
                            <th className='table-header-fix'>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {renderRulesList()}
                    </tbody>
                </table>
            </Row>
        </Container>
    )
}

export default QueryRulesList