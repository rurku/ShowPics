export class FileSystemObject {
  type: string;
  path: string;
  apiPath: string;
  thumbnailPath: string;
  name: string;
  children: FileSystemObject[];
  contentType: string;
  width: number;
  height: number;
  hasSubdirectories: boolean;
  subdirectories: FileSystemObject[];
}

export class FileSystemObjectTypes {
  static readonly FILE = 'FileDto';
  static readonly DIRECTORY = 'DirectoryDto';
}
