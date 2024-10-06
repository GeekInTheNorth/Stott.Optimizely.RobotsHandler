import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { Container } from 'react-bootstrap';
import EnvironmentConfiguration from './EnvironmentConfiguration';

function EnvironmentRobotsSettings(props) {
  
    const [availableEnvironments, setAvailableEnvironments] = useState([])
    
    useEffect(() => {
        getAvailableEnvironments()
    }, []);

    const handleShowFailureToast = (title, description) => props.showToastNotificationEvent && props.showToastNotificationEvent(false, title, description);

    const getAvailableEnvironments = async () => {
        
        setAvailableEnvironments([]);
        
        await axios.get(import.meta.env.VITE_APP_ENVIRONMENT_LIST)
            .then((response) => {
                if (response.data && Array.isArray(response.data)){
                    setAvailableEnvironments(response.data);
                }
                else{
                    handleShowFailureToast('Failure', 'Failed to retrieve robots configuration data.');
                }
            },
            () => {
                handleShowFailureToast('Failure', 'Failed to retrieve robots configuration data.');
            });
    };

    const renderEnvironmentList = () => {
        return availableEnvironments && availableEnvironments.map((environment, index) => {
            return (
                <EnvironmentConfiguration key={index} environment={environment} showToastNotificationEvent={props.showToastNotificationEvent}></EnvironmentConfiguration>
            )
        })
    };

    return(
        <Container className='mt-3'>
            {renderEnvironmentList()}
        </Container>)
}

export default EnvironmentRobotsSettings