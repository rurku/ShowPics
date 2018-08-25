import { Component, OnInit } from '@angular/core';
import { FileSystemObject, FileSystemObjectTypes } from './file-service-dtos';
import { FileService } from './file.service';

@Component({
  selector: 'app-media-browser',
  templateUrl: './media-browser.component.html',
  styleUrls: ['./media-browser.component.css'],
  providers: [FileService]
})
export class MediaBrowserComponent implements OnInit {

  selectedFileSystemObject: FileSystemObject;
  fileSystemObjectTypes = FileSystemObjectTypes;

  constructor() { }

  ngOnInit() {
  }

  onFileSystemObjectSelected(object: FileSystemObject) {
    this.selectedFileSystemObject = object;
  }
}
