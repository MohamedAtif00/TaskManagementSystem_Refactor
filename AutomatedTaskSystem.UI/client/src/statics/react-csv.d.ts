declare module 'react-csv' {
  import * as React from 'react';

  export interface CSVLinkProps {
    data: object[] | string;
    headers?: { label: string; key: string }[];
    filename?: string;
    target?: string;
    uFEFF?: boolean;
    separator?: string;
    enclosingCharacter?: string;
    className?: string;
    style?: React.CSSProperties;
    onClick?: (event: React.MouseEvent<HTMLAnchorElement, MouseEvent>) => void;
    /** This line fixes your error */
    children?: React.ReactNode;
  }

  export class CSVLink extends React.Component<CSVLinkProps> {}
  export class CSVDownload extends React.Component<CSVLinkProps> {}
}
