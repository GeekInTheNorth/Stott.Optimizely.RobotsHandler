import { Form } from 'react-bootstrap'

function RobotsField(props) {
    const { value, onChange } = props;

    const VALUE_NOINDEX = 'noindex';
    const VALUE_NOFOLLOW = 'nofollow';

    const parseRobotsValue = (robotsValue) => {
        const parts = (robotsValue ?? '').split(',').filter(Boolean);
        return {
            noIndex: parts.includes(VALUE_NOINDEX),
            noFollow: parts.includes(VALUE_NOFOLLOW)
        };
    };

    const buildRobotsValue = (noIndex, noFollow) => {
        return [noIndex && VALUE_NOINDEX, noFollow && VALUE_NOFOLLOW].filter(Boolean).join(',');
    };

    const { noIndex, noFollow } = parseRobotsValue(value);

    const handleNoIndexChange = (e) => {
        onChange(buildRobotsValue(e.target.checked, noFollow));
    };

    const handleNoFollowChange = (e) => {
        onChange(buildRobotsValue(noIndex, e.target.checked));
    };

    return (
        <>
            <label>Robots Value</label>
            <div className='border rounded p-3 mb-3'>
                
                <Form.Check type='checkbox' name='NoIndex' label='No Index' checked={noIndex} onChange={handleNoIndexChange} />
                <Form.Check type='checkbox' name='NoFollow' label='No Follow' checked={noFollow} onChange={handleNoFollowChange} />
            </div>
        </>
    )
}

export default RobotsField;
