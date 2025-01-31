import type { ModCommentModel } from '~/models/mods/ModCommentModel';

export interface GetModCommentsResponse {
  totalComments: number;
  comments: ModCommentModel[];
}
