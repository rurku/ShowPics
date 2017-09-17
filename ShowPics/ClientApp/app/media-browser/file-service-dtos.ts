export class FileSystemObject {
  type: string;
  path: string;
  name: string;
  children: FileSystemObject[];
  contentType: string;
}

export class FileSystemObjectTypes {
  static readonly FILE = 'FileDto';
  static readonly DIRECTORY = 'DirectoryDto';
}
