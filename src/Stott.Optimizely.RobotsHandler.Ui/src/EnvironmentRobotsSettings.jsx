import React, { useState, useEffect } from 'react';
import { Container } from 'react-bootstrap';
import EnvironmentConfiguration from './EnvironmentConfiguration';

function EnvironmentRobotsSettings() {
  
    return(
        <Container className='mt-3'>
            <EnvironmentConfiguration environmentName='Integration'></EnvironmentConfiguration>
            <EnvironmentConfiguration environmentName='PreProduction'></EnvironmentConfiguration>
            <EnvironmentConfiguration environmentName='Production'></EnvironmentConfiguration>
        </Container>)
}

export default EnvironmentRobotsSettings