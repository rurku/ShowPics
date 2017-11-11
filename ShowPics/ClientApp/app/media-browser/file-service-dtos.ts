export class FileSystemObject {
  type: string;
  path: string;
  name: string;
  children: FileSystemObject[];
  contentType: string;
  width: number;
  height: number;
}

export class FileSystemObjectTypes {
  static readonly FILE = 'FileDto';
  static readonly DIRECTORY = 'DirectoryDto';
}
