import type { ModSortDirection, ModSortType } from '~/store/mods';

export interface SearchModsRequest {
  text?: string;
  sort?: ModSortType;
  direction?: ModSortDirection;
  author?: string;
  side?: string;
  gameVersion?: string;
  tags?: string[];
}
