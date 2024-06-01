import './App.css'
import 'bootstrap/dist/css/bootstrap.min.css';
import { useState } from 'react'
import { Alert, Col, Container, Row, Toast, ToastContainer } from 'react-bootstrap'
import ConfigurationList from './ConfigurationList'
import AddSiteRobots from './AddSiteRobots';

function App() {

  const [showToastNotification, setShowToastNotification] = useState(false);
  const [toastTitle, setToastTitle] = useState('');
  const [toastDescription, setToastDescription] = useState('');
  const [toastHeaderClass, setToastHeaderClass] = useState('');

  const closeToastNotification = () => setShowToastNotification(false);

  const showToastNotificationEvent = (isSuccess, title, description) => {
    if (isSuccess === true) {
      setToastHeaderClass('bg-success text-white');
    } else {
      setToastHeaderClass('bg-danger text-white');
    }

    setShowToastNotification(false);
    setToastTitle(title);
    setToastDescription(description)
    setShowToastNotification(true);
  };

  return (
    <>
      <Container>
        <Row>
          <Col lg={9} xs={12}>
            <Alert variant='primary' className='p-3'>A default configuration will always be shown for each site to reflect the fallback behaviour of the AddOn.</Alert>
          </Col>
          <Col lg={3} xs={12}>
            <AddSiteRobots showToastNotificationEvent={showToastNotificationEvent}></AddSiteRobots>
          </Col>
        </Row>
      </Container>
      
      <ConfigurationList showToastNotificationEvent={showToastNotificationEvent}></ConfigurationList>
      <ToastContainer className="p-3" position='middle-center'>
        <Toast onClose={closeToastNotification} show={showToastNotification} delay={4000} autohide={true}>
          <Toast.Header className={toastHeaderClass}>
            <strong className="me-auto">{toastTitle}</strong>
          </Toast.Header>
          <Toast.Body className='bg-full-opacity'>{toastDescription}</Toast.Body>
        </Toast>
      </ToastContainer>
    </>
  )
}

export default App
