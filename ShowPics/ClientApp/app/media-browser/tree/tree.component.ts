import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { FileService } from '../file.service';
import { FileSystemObject } from '../file-service-dtos';

@Component({
  selector: 'app-tree',
  templateUrl: './tree.component.html',
  styleUrls: ['./tree.component.css']
})
export class TreeComponent implements OnInit {
  selectedObject: FileSystemObject;
  tree: FileSystemObject;
  @Output() onSelected = new EventEmitter<FileSystemObject>();

  constructor(private fileService: FileService) { }

  getTree(): void {
    this.fileService.getFiles().then(fso => this.tree = fso);
  }

  ngOnInit() {
    this.getTree();
  }

  onSelect(fso: FileSystemObject) {
    this.selectedObject = fso;
    this.onSelected.emit(fso);
  }
}
