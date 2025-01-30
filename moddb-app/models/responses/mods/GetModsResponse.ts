import type { ModDisplayModel } from '~/models/mods/ModDisplayModel';

export interface GetModsResponse {
  totalMods: number;
  mods: ModDisplayModel[];
}
