import { Role } from '@/enums/role';

export interface IAccount {
  id: string;
  username: string;
}

export interface IAccountWithRoles extends IAccount {
  roles: Role[];
}

export interface IAccountExtended extends IAccountWithRoles {
  email: string;
  createdAt: string;
  lastLoginAt: string;
  countryFlag: string;
  avatar: string;
  goldStars: number;
  blueStars: number;
  xp: number;
  lastMatchAt: string;
  flags: number;
  level: number;
  levelStockLimited: number;
  activationLinkGUID: string;
  activationLinkExpiration: Date;
}
