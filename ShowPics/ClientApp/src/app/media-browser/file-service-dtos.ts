export class FileSystemObject {
  type: string;
  path: string;
  thumbnailPath: string;
  name: string;
  children: FileSystemObject[];
  contentType: string;
  width: number;
  height: number;
  subfolders: FileSystemObject[];
}

export class FileSystemObjectTypes {
  static readonly FILE = 'FileDto';
  static readonly DIRECTORY = 'DirectoryDto';
}
