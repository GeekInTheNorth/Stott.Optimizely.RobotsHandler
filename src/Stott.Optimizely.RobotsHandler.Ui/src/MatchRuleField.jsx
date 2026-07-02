import { useEffect, useState } from 'react'

function MatchRuleField(props) {
    const { value, onChange } = props;

    const MATCH_TYPE_NUMBER = 'number';
    const MATCH_TYPE_ANY = 'any';
    const MATCH_TYPE_SPECIFIC = 'specific';

    const NUMBER_MATCH_REGEX = /^(<=|>=|<|>|=)(\d+(\.\d+)?)$/;

    const parseMatchRule = (matchRule) => {
        if (matchRule === '*') {
            return { matchType: MATCH_TYPE_ANY, matchOperator: '>', matchNumber: '', matchValue: '' };
        }

        const match = NUMBER_MATCH_REGEX.exec(matchRule ?? '');
        if (match) {
            return { matchType: MATCH_TYPE_NUMBER, matchOperator: match[1], matchNumber: match[2], matchValue: '' };
        }

        return { matchType: MATCH_TYPE_SPECIFIC, matchOperator: '>', matchNumber: '', matchValue: matchRule ?? '' };
    };

    const buildMatchRule = (matchType, matchOperator, matchNumber, matchValue) => {
        switch (matchType) {
            case MATCH_TYPE_ANY:
                return '*';
            case MATCH_TYPE_NUMBER:
                return ''.concat(matchOperator, matchNumber);
            default:
                return matchValue;
        }
    };

    const initial = parseMatchRule(value);
    const [matchType, setMatchType] = useState(initial.matchType)
    const [matchOperator, setMatchOperator] = useState(initial.matchOperator)
    const [matchNumber, setMatchNumber] = useState(initial.matchNumber)
    const [matchValue, setMatchValue] = useState(initial.matchValue)

    useEffect(() => {
        if (value !== buildMatchRule(matchType, matchOperator, matchNumber, matchValue)) {
            const parsed = parseMatchRule(value);
            setMatchType(parsed.matchType);
            setMatchOperator(parsed.matchOperator);
            setMatchNumber(parsed.matchNumber);
            setMatchValue(parsed.matchValue);
        }
    }, [value]);

    const emitChange = (nextType, nextOperator, nextNumber, nextValue) => {
        onChange(buildMatchRule(nextType, nextOperator, nextNumber, nextValue));
    };

    const handleMatchTypeChange = (e) => {
        const nextType = e.target.value;
        const nextNumber = nextType === MATCH_TYPE_NUMBER && !matchNumber ? '1' : matchNumber;
        setMatchType(nextType);
        setMatchNumber(nextNumber);
        emitChange(nextType, matchOperator, nextNumber, matchValue);
    };

    const handleMatchOperatorChange = (e) => {
        const nextOperator = e.target.value;
        setMatchOperator(nextOperator);
        emitChange(matchType, nextOperator, matchNumber, matchValue);
    };

    const handleMatchNumberChange = (e) => {
        const nextNumber = e.target.value;
        setMatchNumber(nextNumber);
        emitChange(matchType, matchOperator, nextNumber, matchValue);
    };

    const handleMatchValueChange = (e) => {
        const nextValue = e.target.value;
        setMatchValue(nextValue);
        emitChange(matchType, matchOperator, matchNumber, nextValue);
    };

    return (
        <div className='mb-3'>
            <label>Match Rule Type</label>
            <select className='form-control form-select mb-2' name='MatchType' onChange={handleMatchTypeChange} value={matchType}>
                <option value={MATCH_TYPE_ANY}>Any</option>
                <option value={MATCH_TYPE_NUMBER}>Number Match</option>
                <option value={MATCH_TYPE_SPECIFIC}>Specific Value</option>
            </select>
            {matchType !== MATCH_TYPE_ANY &&
                <label>Match Rule Value</label>
            }
            {matchType === MATCH_TYPE_NUMBER &&
                <div className='d-flex gap-2'>
                    <select className='form-control form-select' name='MatchOperator' onChange={handleMatchOperatorChange} value={matchOperator}>
                        <option value='<'>Less Than</option>
                        <option value='<='>Less Than or Equal To</option>
                        <option value='='>Equal To</option>
                        <option value='>'>Greater Than</option>
                        <option value='>='>Greater Than or Equal To</option>
                    </select>
                    <input className='form-control' name='MatchNumber' type='number' onChange={handleMatchNumberChange} value={matchNumber}></input>
                </div>
            }
            {matchType === MATCH_TYPE_SPECIFIC &&
                <input className='form-control' name='MatchValue' type='text' onChange={handleMatchValueChange} value={matchValue}></input>
            }
        </div>
    )
}

export default MatchRuleField;
