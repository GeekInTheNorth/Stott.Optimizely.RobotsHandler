import { useState } from 'react'
import axios from 'axios';
import { Button, Modal } from 'react-bootstrap'

function EditSiteRobots(props) {

    const [showModal, setShowModal] = useState(false)
    const [siteId, setSiteId] = useState(props.siteId ?? '')
    const [siteName, setSiteName] = useState('')
    const [siteRobotsContent, setSiteRobotsContent] = useState('')

    const handleSiteRobotsContentChange = (event) => {
        setSiteRobotsContent(event.target.value);
    }

    const handleShowEditModal = async () => {
        await axios.get(import.meta.env.VITE_APP_ROBOTS_EDIT, { params: { siteId: siteId } })
            .then((response) => {
                if (response.data) {
                    setSiteName(response.data.siteName);
                    setSiteRobotsContent(response.data.robotsContent);
                    setShowModal(true);
                }
                else{
                    // handleShowFailureToast("Get CSP Sources", "Failed to retrieve Content Security Policy Sources.");
                }
            },
            () => {
                // handleShowFailureToast("Error", "Failed to retrieve the Content Security Policy Sources.");
            });
    }

    const handleSaveRobotsContent = async () => {
        let params = new URLSearchParams();
        params.append('siteId', siteId);
        params.append('siteName', siteName);
        params.append('robotsContent', siteRobotsContent);

        await axios.post(import.meta.env.VITE_APP_ROBOTS_SAVE, params)
            .then(() => {
                setShowModal(false);
            },
            () => {
                setShowModal(false);
            });
    }

    const handleCloseModal = () => {
        setShowModal(false);
    }

    return (
        <>
            <Button variant='primary' onClick={handleShowEditModal} className='text-nowrap'>Edit</Button>
            <Modal show={showModal}>
                <Modal.Header closeButton onClick={handleCloseModal}>
                    <Modal.Title>{siteName}</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <div className='mb-3'>
                        <label>Robots.txt Content</label>
                        <textarea className='form-control' name='RobotsContent' cols='60' rows='10' onChange={handleSiteRobotsContentChange}>
                            {siteRobotsContent}
                        </textarea>
                    </div>
                </Modal.Body>
                <Modal.Footer>
                    <Button variant='primary' type='button' onClick={handleSaveRobotsContent}>Save Changes</Button>
                    <Button variant='secondary' type='button' onClick={handleCloseModal}>Cancel</Button>
                </Modal.Footer>
            </Modal>
        </>
    )
}

export default EditSiteRobots

/*

$('.js-save-button').click(function () {
    let siteName = $('.js-modal-title').text();
    let siteId = $('.js-modal-siteid').text();
    let robotsContent = $('.js-modal-robots-content').val();

    $.post('/stott.robotshandler/save/', { siteId: siteId, siteName: siteName, robotsContent: robotsContent })
        .done(function () {
            let saveSuccess = '<div class="alert alert-success alert-dismissible fade show m-3" role="alert">'
                + '<strong>Success!</strong> Your robots.txt content changes were successfully applied.'
                + '<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>'
                + '</div>';
            $(saveSuccess).insertAfter('.js-header');
            myModal.toggle();
        })
        .fail(function () {
            let saveError = '<div class="alert alert-danger alert-dismissible fade show" role="alert">'
                + '<strong>Failure!</strong> An error was encountered when trying to save your robots.txt content.'
                + '<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>'
                + '</div>';
            $(saveError).insertAfter('.js-header');
            myModal.toggle();
        });
});

$('.js-edit-button').click(function () {
    let siteId = $(this).data('siteid');
    $.get('/stott.robotshandler/details/', { siteId: siteId })
        .done(function (data) {
            $('.js-modal-title').html(data.siteName);
            $('.js-modal-siteid').html(data.siteId);
            $('.js-modal-robots-content').val(data.robotsContent);

            myModal.toggle();
        })
        .fail(function () {
            let loadError = '<div class="alert alert-danger alert-dismissible fade show m-3" role="alert">'
                + '<strong>Failure!</strong> An error was encountered when trying to retrieve your robots.txt content.'
                + '<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>'
                + '</div>';
            $(loadError).insertAfter('.js-header');
        });
});


<div id="edit-robots-content-modal" class="modal" tabindex="-1">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title js-modal-title">Modal title</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
            </div>
            <div class="modal-body">
                <div class="modal-body">

                    <input type="hidden" name="SiteId" class="js-modal-siteid" />

                    <div class="mb-3">
                        <label for="Robots-Content">Robots.txt Content</label>
                        <textarea class="form-control js-modal-robots-content" name="RobotsContent" cols="60" rows="10"></textarea>
                    </div>

                </div>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-primary text-nowrap js-save-button">Save changes</button>
                <button type="button" class="btn btn-secondary text-nowrap" data-bs-dismiss="modal">Close</button>
            </div>
        </div>
    </div>
</div>
*/