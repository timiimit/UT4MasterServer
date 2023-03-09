import { GameServerTrust } from '@/enums/game-server-trust';
import { IMatch } from './match';

export interface IHub {
  id: string;
  serverName: string;
  serverTrustLevel: GameServerTrust;
  matches: IMatch[];
  totalPlayers: number;
  customMatchNames: Record<string, string>;
}
