import type { ModCommentModel } from './ModCommentModel';
import type { ModDisplayModel } from './ModDisplayModel';

export interface LatestModCommentModel {
  comment: ModCommentModel;
  mod: ModDisplayModel;
}
