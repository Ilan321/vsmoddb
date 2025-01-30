import type { TagModel } from '../TagModel';

export interface ModDetailsModel {
  id: number;
  name: string;
  summary: string;
  description?: string;
  timeCreatedUtc: string;
  timeUpdatedUtc: string;
  urlAlias?: string;
  tags: TagModel[];
  author: string;
  side: string;
  downloads: number;
}
