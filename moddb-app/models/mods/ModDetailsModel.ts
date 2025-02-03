import type { TagModel } from '../TagModel';
import type { ModRelease } from './ModRelease';

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
  releases: ModRelease[];
  sourceCodeUrl?: string;
  homepageUrl?: string;
  issuerTrackerUrl?: string;
  wikiUrl?: string;
}
