import { format } from 'date-fns';
import React, { useEffect, useMemo, useState } from 'react';
import { Button } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import { CopyButton } from '../../../sharedCommonComponents/components/CopyButton';
import { PagedTable } from '../../../sharedCommonComponents/components/PagedTable';
import { resolveText } from '../../../sharedCommonComponents/helpers/Globalizer';
import PagedTableLoader from '../../../sharedCommonComponents/helpers/PagedTableLoader';
import { Models } from '../../types/models';

interface AssignedProfilesTableProps {
    latestAssignedId?: string;
}

export const AssignedProfilesTable = (props: AssignedProfilesTableProps) => {

    const [ profiles, setProfiles ] = useState<Models.AssignerControlledProfile[]>([]);
    const navigate = useNavigate();
    const loader = useMemo(() => new PagedTableLoader(
        'api/assigner/assigned-ids', 
        resolveText("AssignerControlledProfiles_CouldNotLoad"),
        setProfiles), []);

    useEffect(() => {
        loader.load(0, 10);
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [ props.latestAssignedId ]);

    return (
        <PagedTable
            onPageChanged={loader.load}
            className="align-middle"
        >
            <thead>
                <tr>
                    <td>{resolveText("AssignerControlledProfile_CreationDate")}</td>
                    <td>{resolveText("AssignerControlledProfile_Id")}</td>
                    <td></td>
                </tr>
            </thead>
            <tbody>
            {profiles.length > 0
            ? profiles.map(profile => (
                <tr key={profile.id}>
                    <td>{format(new Date(profile.creationDate), 'yyyy-MM-dd HH:mm')}</td>
                    <td>
                        <strong>{profile.id}</strong>
                        <CopyButton
                            value={profile.id}
                            size='xs'
                            className='ms-3'
                        />
                    </td>
                    <td>
                        <Button
                            variant='primary'
                            onClick={() => navigate(`/print/certificate/${profile.id}`)}
                        >
                            {resolveText("AssignerControlledProfile_DownloadCertificate")}
                        </Button>
                    </td>
                </tr>
            ))
            : <tr>
                <td colSpan={3} className='text-center'>{resolveText("NoEntries")}</td>
            </tr>}
            </tbody>
        </PagedTable>
    );

}