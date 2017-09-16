export class TreeElement {
  type: TreeElementType;
  path: string;
  name: string;

  constructor(type: TreeElementType, name: string) {
    this.type = type;
    this.name = name;
  }
}

export enum TreeElementType {
  File,
  Folder
}
