import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { FileService } from '../file.service';
import { FileSystemObject } from '../file-service-dtos';
import { TREE_ACTIONS, KEYS, IActionMapping, ITreeOptions } from 'angular-tree-component';

@Component({
  selector: 'app-tree',
  templateUrl: './tree.component.html',
  styleUrls: ['./tree.component.css']
})
export class TreeComponent implements OnInit {
  selectedObject: FileSystemObject;
  tree: FileSystemObject[];

  options: ITreeOptions = {
    idField: 'path'
  }


  @Output() onSelected = new EventEmitter<FileSystemObject>();

  constructor(private fileService: FileService) { }

  getTree(): void {
    this.fileService.getFiles().then(fso => this.tree = fso.children);
  }

  ngOnInit() {
    this.getTree();
  }

  onSelect(fso: FileSystemObject) {
    this.selectedObject = fso;
    this.onSelected.emit(fso);
  }
}
