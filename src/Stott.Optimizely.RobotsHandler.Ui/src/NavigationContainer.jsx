import React, { useState, useEffect } from 'react';
import ConfigurationList from './ConfigurationList';
import EnvironmentRobotsSettings from './EnvironmentRobotsSettings';

function NavigationContainer() {

    const [showRobotsList, setShowRobotsList] = useState(false);
    const [showEnvironmentRobots, setShowEnvironmentRobots] = useState(false);
    const [containerTitle, setContainerTitle] = useState('Robots.txt List');

    const handleHandleShowToastNotification = (isSuccess, title, description) => props.showToastNotificationEvent && props.showToastNotificationEvent(isSuccess, title, description);

    const handleSelect = (key) => {
        setContainerTitle('');
        setShowRobotsList(false);
        setShowEnvironmentRobots(false);

        switch(key){
            case 'robots-list':
                setContainerTitle('Robots.txt List');
                setShowRobotsList(true);
                break;
            case 'site-robots':
                setContainerTitle('Site Robots');
                setShowEnvironmentRobots(true);
                break;
            default:
                // No default required
                break;
        }
    }

    useEffect(() => {
        const handleHashChange = () => {
            var hash = window.location.hash?.substring(1);
            if (hash && hash !== '') {
                handleSelect(hash);
            }
            else {
                handleSelect('robots-list');
            }
        };

        window.addEventListener('hashchange', handleHashChange);
        handleHashChange();

        return () => {
            window.removeEventListener('hashchange', handleHashChange);
        }
    });

    return (
        <>
            <div className="container-fluid p-2 bg-dark text-light">
                <p className="my-0 h5">Stott Robots Handler | {containerTitle}</p>
            </div>
            <div className="container-fluid security-app-container">
                { showRobotsList ? <ConfigurationList showToastNotificationEvent={handleHandleShowToastNotification}></ConfigurationList> : null }
                { showEnvironmentRobots ? <EnvironmentRobotsSettings showToastNotificationEvent={handleHandleShowToastNotification}></EnvironmentRobotsSettings> : null }
            </div>
        </>
    )
}

export default NavigationContainer