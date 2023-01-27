import { AccountFlag } from "@/enums/account-flag";

export interface IAccount {
    id: string;
    displayName: string;
    email?: string;
    name?: string;
    failedLoginAttempts?: number;
    lastLogin?: string;
    numberOfDisplayNameChanges?: number;
    ageGroup?: string;
    headless?: boolean;
    country?: string;
    lastName?: string;
    preferredLanguage?: string;
    canUpdateDisplayName?: boolean;
    tfaEnabled?: boolean;
    emailVerified?: boolean;
    minorVerified?: boolean;
    minorExpected?: boolean;
    minorStatus?: string;
    cabinedMode?: boolean;
    hasHashedEmail?: boolean
    flags?: AccountFlag[];
}
