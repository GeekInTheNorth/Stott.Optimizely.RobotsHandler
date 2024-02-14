import './App.css'
import 'bootstrap/dist/css/bootstrap.min.css';
import { useState } from 'react'
import { Toast, ToastContainer } from 'react-bootstrap'
import ConfigurationList from './ConfigurationList'

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
