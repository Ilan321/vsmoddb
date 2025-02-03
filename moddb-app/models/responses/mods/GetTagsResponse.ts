import type { TagModel } from '~/models/TagModel';

export interface GetTagsResponse {
  tags: TagModel[];
  gameVersions: TagModel[];
}
