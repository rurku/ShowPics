import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { FileService } from '../file.service';
import { FileSystemObject, FileSystemObjectTypes } from '../file-service-dtos';
import { TREE_ACTIONS, KEYS, IActionMapping, ITreeOptions, TreeNode } from 'angular-tree-component';
import { pipe, Observable, of } from 'rxjs';
import { map, tap } from 'rxjs/operators';

@Component({
  selector: 'app-tree',
  templateUrl: './tree.component.html',
  styleUrls: ['./tree.component.css']
})
export class TreeComponent implements OnInit {
  selectedObject: FileSystemObject;
  tree: FileSystemObject[];

  options: ITreeOptions = {
    idField: 'path',
    getChildren: (treeNode: TreeNode) => this.getChildren(treeNode.data).toPromise(),
    hasChildrenField: 'hasSubdirectories',
    childrenField: 'subdirectories'
  }

  @Output() onSelected = new EventEmitter<FileSystemObject>();

  constructor(private fileService: FileService) { }

  getChildren(node: FileSystemObject): Observable<FileSystemObject[]> {
    if (node.children == null) {
      return this.fileService.getFiles(node.apiPath, 1)
        .pipe(
          tap((fso: FileSystemObject) => node.children = fso.children),
          map((fso: FileSystemObject) => fso.children.filter(x => x.type === FileSystemObjectTypes.DIRECTORY))
        );
    }
    else {
      return of(node.children.filter(x => x.type === FileSystemObjectTypes.DIRECTORY))
    }
  }

  getTree(): void {
    this.fileService.getFiles(FileService.rootUrl, 1).subscribe(fso => {
      this.tree = fso.children
    });
  }

  ngOnInit() {
    this.getTree(); 
  }

  onSelect(fso: FileSystemObject) {
    this.getChildren(fso).subscribe(() => {
      this.selectedObject = fso;
      this.onSelected.emit(fso);
    })
  }
}
