import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MediaItemNav } from '../shared/components/media-item-nav/media-item-nav';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, MediaItemNav],
  templateUrl: './app-shell.html',
  styleUrl: './app-shell.scss',
})
export class AppShell {}
