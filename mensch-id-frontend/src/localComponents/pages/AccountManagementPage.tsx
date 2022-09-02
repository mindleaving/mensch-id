import React, { useEffect, useState } from 'react';
import { Badge, Button, Table } from 'react-bootstrap';
import { DeleteButton } from '../../sharedCommonComponents/components/DeleteButon';
import { deleteObject } from '../../sharedCommonComponents/helpers/DeleteHelpers';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { loadObject } from '../../sharedCommonComponents/helpers/LoadingHelpers';
import { ChangePasswordForm } from '../components/ChangePasswordForm';
import { AccountType } from '../types/enums.d';
import { Models } from '../types/models';

interface AccountManagementPageProps {}

export const AccountManagementPage = (props: AccountManagementPageProps) => {

    const [ isLoading, setIsLoading] = useState<boolean>(true);
    const [ accounts, setAccounts ] = useState<Models.Account[]>([]);
    const [ selectedAccount, setSelectedAccount ] = useState<Models.Account>();
    const [ isDeleting, setIsDeleting ] = useState<boolean>(false);


    const loadAccounts = async () => {
        setIsLoading(true);
        await loadObject(
            'api/accounts', {},
            resolveText("Accounts_CouldNotLoad"),
            setAccounts,
            undefined,
            () => setIsLoading(false)
        )
    };

    useEffect(() => {
        loadAccounts();
    }, []);

    const deleteAccount = async (accountId: string) => {
        setIsDeleting(true);
        await deleteObject(
            `api/accounts/${accountId}`, {},
            resolveText("Account_SuccessfullyDeleted"),
            resolveText("Account_CouldNotDelete"),
            () => {
                setAccounts(state => state.filter(x => x.id !== accountId));
            },
            () => setIsDeleting(false)
        );
    }

    const formatAccount = (account: Models.Account) => {
        switch(account.accountType) {
            case AccountType.External:
                const externalAccount = account as Models.ExternalAccount;
                return resolveText(`LoginProvider_${externalAccount.loginProvider}`);
            case AccountType.LocalAnonymous:
                return '';
            case AccountType.Local:
            case AccountType.Assigner:
                const localAccount = account as Models.LocalAccount;
                return (<>
                    {localAccount.email}
                    {localAccount.isEmailVerified
                    ? <Badge 
                        pill 
                        bg="light"
                        text="success"
                        className='ms-2'
                    >
                        <i className='fa fa-check green'/> {resolveText("Verified")}
                    </Badge> 
                    : null}
                </>);
            default:
                return '';
        }
    }

    if(isLoading) {
        return (<h3>{resolveText("Loading...")}</h3>);
    }

    const localAccountTypes = [ AccountType.LocalAnonymous, AccountType.Local, AccountType.Assigner ];
    return (
        <>
            <h1>{resolveText("AccountManagement")}</h1>

            <h3>{resolveText("Accounts")}</h3>
            <Table>
                <tbody>
                    {accounts.length > 0
                    ? accounts.map(account => (
                        <tr key={account.id}>
                            <td>{resolveText(`AccountType_${account.accountType}`)}</td>
                            <td>{formatAccount(account)}</td>
                            <td>
                                {localAccountTypes.includes(account.accountType)
                                ? <Button
                                    onClick={() => setSelectedAccount(account)}
                                >
                                    {resolveText("Account_ChangePassword")}
                                </Button>
                                : null}
                            </td>
                            <td>
                                <DeleteButton
                                    requireConfirm
                                    confirmDialogTitle={resolveText("Account_ConfirmDelete_Title")}
                                    confirmDialogMessage={resolveText("Account_ConfirmDelete_Message")}
                                    onClick={() => deleteAccount(account.id)}
                                    isDeleting={isDeleting}
                                />
                            </td>
                        </tr>
                    ))
                    : <tr>
                        <td colSpan={3} className="text-center">{resolveText("NoEntries")}</td>
                    </tr>}
                </tbody>
            </Table>

            {selectedAccount && localAccountTypes.includes(selectedAccount.accountType)
            ? <>
                <h3>{resolveText("ChangePassword")}</h3>
                <ChangePasswordForm
                    accountId={selectedAccount.id}
                    onPasswordChanged={() => setSelectedAccount(undefined)}
                />
            </>
            : null}
        </>
    );

}