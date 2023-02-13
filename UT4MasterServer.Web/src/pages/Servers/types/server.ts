import { GameServerTrust } from '@/enums/game-server-trust';
import { IMatch } from './match';

export interface IServer extends IMatch {
  serverTrustLevel: GameServerTrust;
}
