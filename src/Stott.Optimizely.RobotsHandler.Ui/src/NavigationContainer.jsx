import { useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import ConfigurationList from './ConfigurationList';
import EnvironmentRobotsSettings from './EnvironmentRobotsSettings';
import LlmsConfigurationList from './LlmsConfigurationList';
import TokenManagement from './TokenManagement';
import MarkdownMappingList from './MarkdownMappingList';
import MarkdownSettings from './MarkdownSettings';

function NavigationContainer(props) {

    const [showRobotsList, setShowRobotsList] = useState(false);
    const [showLlmsList, setShowLlmsList] = useState(false);
    const [showEnvironmentRobots, setShowEnvironmentRobots] = useState(false);
    const [showOpalTools, setShowOpalTools] = useState(false);
    const [showMarkdownMapping, setShowMarkdownMapping] = useState(false);
    const [containerTitle, setContainerTitle] = useState('Robots.txt Files');

    const handleSelect = (key) => {
        setContainerTitle('');
        setShowRobotsList(false);
        setShowLlmsList(false);
        setShowMarkdownMapping(false);
        setShowEnvironmentRobots(false);
        setShowOpalTools(false);

        switch(key){
            case 'robots-files':
                setContainerTitle('Robots.txt Files');
                setShowRobotsList(true);
                break;
            case 'llms-files':
                setContainerTitle('LLMS.txt Files');
                setShowLlmsList(true);
                break;
            case 'markdown-mapping':
                setContainerTitle('Markdown Mapping');
                setShowMarkdownMapping(true);
                break;
            case 'environment-robots':
                setContainerTitle('Environment Robots');
                setShowEnvironmentRobots(true);
                break;
            case 'api-tokens':
                setContainerTitle('API Tokens');
                setShowOpalTools(true);
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
                { showOpalTools ? <TokenManagement showToastNotificationEvent={props.showToastNotificationEvent}></TokenManagement> : null }
                { showMarkdownMapping ? <MarkdownSettings showToastNotificationEvent={props.showToastNotificationEvent}></MarkdownSettings> : null }
                { showMarkdownMapping ? <MarkdownMappingList showToastNotificationEvent={props.showToastNotificationEvent}></MarkdownMappingList> : null }
            </div>
        </>
    );
}

NavigationContainer.propTypes = {
    showToastNotificationEvent: PropTypes.func.isRequired
};

export default NavigationContainer;