export class TreeElement {
  type: TreeElementType;
  path: string;
  name: string;
}

export enum TreeElementType {
  File,
  Folder
}
