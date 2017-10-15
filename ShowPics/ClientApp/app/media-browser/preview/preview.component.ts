import { Component, OnInit, Input } from '@angular/core';
import { FileSystemObject } from '../file-service-dtos';
import { FileService } from '../file.service';

@Component({
  selector: 'app-preview',
  templateUrl: './preview.component.html',
  styleUrls: ['./preview.component.css']
})
export class PreviewComponent implements OnInit {

  constructor(public fileService: FileService) { }

  @Input() fileSystemObject: FileSystemObject;

  ngOnInit() {
  }

}
