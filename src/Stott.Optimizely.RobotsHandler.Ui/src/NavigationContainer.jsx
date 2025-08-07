import React, { useState, useEffect } from 'react';
import ConfigurationList from './ConfigurationList';
import EnvironmentRobotsSettings from './EnvironmentRobotsSettings';
import LlmsConfigurationList from './LlmsConfigurationList';

function NavigationContainer(props) {

    const [showRobotsList, setShowRobotsList] = useState(false);
    const [showLlmsList, setShowLlmsList] = useState(false);
    const [showEnvironmentRobots, setShowEnvironmentRobots] = useState(false);
    const [containerTitle, setContainerTitle] = useState('Robots.txt Files');

    const handleSelect = (key) => {
        setContainerTitle('');
        setShowRobotsList(false);
        setShowLlmsList(false);
        setShowEnvironmentRobots(false);

        switch(key){
            case 'robots-files':
                setContainerTitle('Robots.txt Files');
                setShowRobotsList(true);
                break;
            case 'llms-files':
                setContainerTitle('LLMS.txt Files');
                setShowLlmsList(true);
                break;
            case 'environment-robots':
                setContainerTitle('Environment Robots');
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
                handleSelect('robots-files');
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
                { showRobotsList ? <ConfigurationList showToastNotificationEvent={props.showToastNotificationEvent}></ConfigurationList> : null }
                { showEnvironmentRobots ? <EnvironmentRobotsSettings showToastNotificationEvent={props.showToastNotificationEvent}></EnvironmentRobotsSettings> : null }
                { showLlmsList ? <LlmsConfigurationList showToastNotificationEvent={props.showToastNotificationEvent}></LlmsConfigurationList> : null }
            </div>
        </>
    )
}

export default NavigationContainer